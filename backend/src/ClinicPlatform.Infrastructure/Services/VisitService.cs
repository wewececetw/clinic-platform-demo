using ClinicPlatform.Application.Common;
using ClinicPlatform.Application.Features.Visit;
using ClinicPlatform.Application.Features.Workflow;
using ClinicPlatform.Domain.Entities;
using ClinicPlatform.Domain.Enums;
using ClinicPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicPlatform.Infrastructure.Services;

public class VisitService(ClinicDbContext db, IWorkflowEngine workflowEngine) : IVisitService
{
    public async Task<Result<VisitStatusDto>> GetStatusAsync(Guid clinicId, Guid visitId)
    {
        var visit = await db.Visits
            .Include(v => v.CurrentStep)
            .FirstOrDefaultAsync(v => v.ClinicId == clinicId && v.Id == visitId);

        if (visit is null)
            return Result<VisitStatusDto>.Fail("找不到該就診紀錄");

        return Result<VisitStatusDto>.Ok(new VisitStatusDto(
            visit.Id,
            visit.CurrentStep?.StepCode ?? "unknown",
            visit.CurrentStep?.DisplayName ?? "未知",
            visit.QueueNumber ?? 0,
            visit.Status.ToString(),
            visit.NeedsMedication,
            visit.CheckedInAt,
            visit.CompletedAt));
    }

    public async Task<Result<List<VisitEventDto>>> GetEventsAsync(Guid clinicId, Guid visitId)
    {
        var visitExists = await db.Visits
            .AnyAsync(v => v.ClinicId == clinicId && v.Id == visitId);

        if (!visitExists)
            return Result<List<VisitEventDto>>.Fail("找不到該就診紀錄");

        var events = await db.VisitEvents
            .Include(e => e.FromStep)
            .Include(e => e.ToStep)
            .Include(e => e.TriggeredByUser)
            .Where(e => e.ClinicId == clinicId && e.VisitId == visitId)
            .OrderBy(e => e.CreatedAt)
            .Select(e => new VisitEventDto(
                e.FromStep != null ? e.FromStep.StepCode : "",
                e.ToStep != null ? e.ToStep.StepCode : "",
                e.TriggerType.ToString(),
                e.TriggeredByUser != null ? e.TriggeredByUser.DisplayName : null,
                e.CreatedAt))
            .ToListAsync();

        return Result<List<VisitEventDto>>.Ok(events);
    }

    public async Task<Result> StartConsultAsync(StartConsultRequest request, Guid doctorUserId)
    {
        var visit = await db.Visits
            .Include(v => v.CurrentStep)
            .FirstOrDefaultAsync(v => v.ClinicId == request.ClinicId && v.Id == request.VisitId);

        if (visit is null)
            return Result.Fail("找不到該就診紀錄");

        // 找到 consulting 步驟
        var consultingStep = await db.WorkflowSteps
            .FirstOrDefaultAsync(s => s.WorkflowDefinitionId == visit.WorkflowDefinitionId
                && s.StepCode == "consulting");

        if (consultingStep is null)
            return Result.Fail("找不到看診步驟");

        var previousStepId = visit.CurrentStepId;

        // 更新 visit 的當前步驟
        visit.CurrentStepId = consultingStep.Id;
        visit.DoctorId = doctorUserId;
        visit.UpdatedAt = DateTime.UtcNow;

        // 寫入 VisitEvent
        var visitEvent = new VisitEvent
        {
            Id = Guid.NewGuid(),
            ClinicId = request.ClinicId,
            VisitId = visit.Id,
            FromStepId = previousStepId,
            ToStepId = consultingStep.Id,
            TriggerType = TriggerType.Manual,
            TriggeredByUserId = doctorUserId,
            CreatedAt = DateTime.UtcNow
        };
        db.VisitEvents.Add(visitEvent);

        await db.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> CompleteConsultAsync(CompleteConsultRequest request, Guid doctorUserId)
    {
        var visit = await db.Visits
            .FirstOrDefaultAsync(v => v.ClinicId == request.ClinicId && v.Id == request.VisitId);

        if (visit is null)
            return Result.Fail("找不到該就診紀錄");

        visit.NeedsMedication = request.NeedsMedication;
        visit.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        // 呼叫 WorkflowEngine 推進到下一步
        return await workflowEngine.AdvanceAsync(request.ClinicId, request.VisitId, doctorUserId);
    }

    public async Task<Result> AdvanceStepAsync(Guid clinicId, Guid visitId, Guid callerUserId)
    {
        return await workflowEngine.AdvanceAsync(clinicId, visitId, callerUserId);
    }
}
