using ClinicPlatform.Application.Common;
using ClinicPlatform.Application.Features.Notifications;
using ClinicPlatform.Application.Features.Workflow;
using ClinicPlatform.Domain.Entities;
using ClinicPlatform.Domain.Enums;
using ClinicPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicPlatform.Infrastructure.Services;

/// <summary>
/// 簡化版 Workflow Engine（Demo 用）
/// 完整版含條件式跳轉引擎、JSON 規則評估、複合條件支援
/// 保留 SignalR notifier 推播（展示即時通訊架構）
/// </summary>
public class WorkflowEngine(ClinicDbContext db, INotificationPublisher notifier) : IWorkflowEngine
{
    public async Task<Result> AdvanceAsync(Guid clinicId, Guid visitId, Guid? triggeredByUserId)
    {
        var visit = await db.Visits
            .Include(v => v.CurrentStep)
            .FirstOrDefaultAsync(v => v.Id == visitId && v.ClinicId == clinicId);

        if (visit is null)
            return Result.Fail("找不到該門診紀錄");

        if (visit.CurrentStepId is null)
            return Result.Fail("門診流程尚未開始");

        // Demo 版：依 step_order 線性推進到下一步
        var nextStep = await db.WorkflowSteps
            .Where(s => s.WorkflowDefinitionId == visit.WorkflowDefinitionId
                && s.StepOrder > visit.CurrentStep!.StepOrder)
            .OrderBy(s => s.StepOrder)
            .FirstOrDefaultAsync();

        if (nextStep is null)
            return Result.Fail("已是最後步驟");

        var fromStepId = visit.CurrentStepId;
        visit.CurrentStepId = nextStep.Id;
        visit.CurrentStep = nextStep;
        visit.UpdatedAt = DateTime.UtcNow;

        if (nextStep.StepCode == "completed")
        {
            visit.Status = VisitStatus.Completed;
            visit.CompletedAt = DateTime.UtcNow;
        }

        db.VisitEvents.Add(new VisitEvent
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            VisitId = visitId,
            FromStepId = fromStepId,
            ToStepId = nextStep.Id,
            TriggeredByUserId = triggeredByUserId,
            TriggerType = triggeredByUserId.HasValue ? TriggerType.Manual : TriggerType.System,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        // 推播 step 推進事件（保留 SignalR 整合）
        await notifier.PublishVisitStepChangedAsync(visitId, nextStep.StepCode, nextStep.DisplayName);
        await notifier.PublishQueueUpdatedAsync(clinicId, "Consulting", "step_advanced");

        if (nextStep.AutoAdvance)
            return await AdvanceAsync(clinicId, visitId, triggeredByUserId);

        return Result.Ok();
    }
}
