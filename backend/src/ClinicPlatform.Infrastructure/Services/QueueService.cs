using ClinicPlatform.Application.Common;
using ClinicPlatform.Application.Features.Notifications;
using ClinicPlatform.Application.Features.Queue;
using ClinicPlatform.Domain.Entities;
using ClinicPlatform.Domain.Enums;
using ClinicPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicPlatform.Infrastructure.Services;

public class QueueService(ClinicDbContext db, INotificationPublisher notifier) : IQueueService
{
    public async Task<Result<List<QueueEntryDto>>> GetQueueAsync(Guid clinicId, string queueType)
    {
        if (!Enum.TryParse<QueueType>(queueType, ignoreCase: true, out var parsedType))
            return Result<List<QueueEntryDto>>.Fail("無效的佇列類型");

        var entries = await db.QueueEntries
            .Include(q => q.Visit)
                .ThenInclude(v => v.Patient)
            .Where(q => q.ClinicId == clinicId
                && q.QueueType == parsedType
                && q.Status == QueueEntryStatus.Waiting)
            .OrderByDescending(q => q.Priority)
            .ThenBy(q => q.CreatedAt)
            .Select(q => new QueueEntryDto(
                q.VisitId,
                q.QueueNumber,
                q.Visit.Patient.FullName ?? "匿名",
                q.Priority,
                q.Status.ToString(),
                q.Visit.CheckedInAt))
            .ToListAsync();

        return Result<List<QueueEntryDto>>.Ok(entries);
    }

    public async Task<Result<QueuePositionDto>> GetPositionAsync(Guid clinicId, Guid visitId)
    {
        var entry = await db.QueueEntries
            .FirstOrDefaultAsync(q => q.ClinicId == clinicId && q.VisitId == visitId);

        if (entry is null)
            return Result<QueuePositionDto>.Fail("找不到該候診紀錄");

        var position = await db.QueueEntries
            .CountAsync(q => q.ClinicId == clinicId
                && q.QueueType == entry.QueueType
                && q.Status == QueueEntryStatus.Waiting
                && (q.Priority > entry.Priority
                    || (q.Priority == entry.Priority && q.CreatedAt < entry.CreatedAt)));

        var totalWaiting = await db.QueueEntries
            .CountAsync(q => q.ClinicId == clinicId
                && q.QueueType == entry.QueueType
                && q.Status == QueueEntryStatus.Waiting);

        return Result<QueuePositionDto>.Ok(
            new QueuePositionDto(position + 1, entry.QueueNumber, totalWaiting));
    }

    public async Task<Result<QueueEntryDto>> CallNextAsync(CallNextRequest request, Guid callerUserId)
    {
        if (!Enum.TryParse<QueueType>(request.QueueType, ignoreCase: true, out var parsedType))
            return Result<QueueEntryDto>.Fail("無效的佇列類型");

        var entry = await db.QueueEntries
            .Include(q => q.Visit)
                .ThenInclude(v => v.Patient)
            .Where(q => q.ClinicId == request.ClinicId
                && q.QueueType == parsedType
                && q.Status == QueueEntryStatus.Waiting)
            .OrderByDescending(q => q.Priority)
            .ThenBy(q => q.CreatedAt)
            .FirstOrDefaultAsync();

        if (entry is null)
            return Result<QueueEntryDto>.Fail("目前佇列中沒有等待的病患");

        // 更新 QueueEntry 狀態
        entry.Status = QueueEntryStatus.Called;
        entry.CalledAt = DateTime.UtcNow;

        // 更新 Visit：若有指定 RoomId 則一併更新
        var visit = entry.Visit;
        var fromStepId = visit.CurrentStepId;

        // 找到叫號對應的 workflow step（StepCode = "called"）
        var calledStep = await db.WorkflowSteps
            .FirstOrDefaultAsync(s => s.WorkflowDefinitionId == visit.WorkflowDefinitionId
                && s.StepCode == "called");

        if (calledStep is not null)
            visit.CurrentStepId = calledStep.Id;

        if (request.RoomId.HasValue)
            visit.RoomId = request.RoomId.Value;

        visit.UpdatedAt = DateTime.UtcNow;

        // 寫入 VisitEvent
        var visitEvent = new VisitEvent
        {
            Id = Guid.NewGuid(),
            ClinicId = request.ClinicId,
            VisitId = visit.Id,
            FromStepId = fromStepId,
            ToStepId = calledStep?.Id,
            TriggerType = TriggerType.Manual,
            TriggeredByUserId = callerUserId,
            Notes = "叫號",
            CreatedAt = DateTime.UtcNow
        };
        db.VisitEvents.Add(visitEvent);

        await db.SaveChangesAsync();

        // 推播：佇列變動 + 病患被叫號 + visit step 前進
        await notifier.PublishQueueUpdatedAsync(request.ClinicId, request.QueueType, "called");
        if (calledStep is not null)
        {
            await notifier.PublishVisitStepChangedAsync(visit.Id, calledStep.StepCode, calledStep.DisplayName);
        }
        if (visit.RoomId.HasValue)
        {
            var roomName = await db.Rooms
                .Where(r => r.Id == visit.RoomId.Value)
                .Select(r => r.Name)
                .FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(roomName))
            {
                await notifier.PublishPatientCalledAsync(visit.Id, roomName);
            }
        }

        var dto = new QueueEntryDto(
            entry.VisitId,
            entry.QueueNumber,
            visit.Patient.FullName ?? "匿名",
            entry.Priority,
            entry.Status.ToString(),
            visit.CheckedInAt);

        return Result<QueueEntryDto>.Ok(dto);
    }

    public async Task<Result> SkipAsync(Guid clinicId, Guid visitId, Guid callerUserId)
    {
        var entry = await db.QueueEntries
            .FirstOrDefaultAsync(q => q.ClinicId == clinicId && q.VisitId == visitId);

        if (entry is null)
            return Result.Fail("找不到該候診紀錄");

        if (entry.Status != QueueEntryStatus.Waiting)
            return Result.Fail("只能跳過等待中的候診紀錄");

        entry.Status = QueueEntryStatus.Skipped;
        entry.SkippedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        await notifier.PublishQueueUpdatedAsync(clinicId, entry.QueueType.ToString(), "skipped");

        return Result.Ok();
    }
}
