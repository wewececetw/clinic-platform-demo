using System.Text.Json;
using ClinicPlatform.Application.Common;
using ClinicPlatform.Application.Features.Notifications;
using ClinicPlatform.Application.Features.Workflow;
using ClinicPlatform.Domain.Entities;
using ClinicPlatform.Domain.Enums;
using ClinicPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicPlatform.Infrastructure.Services;

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

        var transitions = await db.WorkflowTransitions
            .Include(t => t.ToStep)
            .Where(t => t.WorkflowDefinitionId == visit.WorkflowDefinitionId
                && t.FromStepId == visit.CurrentStepId.Value)
            .OrderByDescending(t => t.Priority)
            .ToListAsync();

        if (transitions.Count == 0)
            return Result.Fail("目前步驟無可用轉移路線");

        WorkflowTransition? matched = null;

        foreach (var transition in transitions)
        {
            if (string.IsNullOrEmpty(transition.ConditionJson))
            {
                matched ??= transition; // 無條件的作為 fallback
                continue;
            }

            if (EvaluateCondition(transition.ConditionJson, visit))
            {
                matched = transition;
                break;
            }
        }

        if (matched is null)
            return Result.Fail("無符合條件的轉移路線");

        var fromStepId = visit.CurrentStepId;
        visit.CurrentStepId = matched.ToStepId;
        visit.CurrentStep = matched.ToStep;
        visit.UpdatedAt = DateTime.UtcNow;

        if (matched.ToStep.StepCode == "completed")
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
            ToStepId = matched.ToStepId,
            TriggeredByUserId = triggeredByUserId,
            TriggerType = triggeredByUserId.HasValue ? TriggerType.Manual : TriggerType.System,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        // 推播 step 推進事件 + 診所佇列變動
        await notifier.PublishVisitStepChangedAsync(visitId, matched.ToStep.StepCode, matched.ToStep.DisplayName);
        await notifier.PublishQueueUpdatedAsync(clinicId, "Consulting", "step_advanced");

        // 新步驟若 AutoAdvance，遞迴推進
        if (matched.ToStep.AutoAdvance)
        {
            return await AdvanceAsync(clinicId, visitId, triggeredByUserId);
        }

        return Result.Ok();
    }

    private static bool EvaluateCondition(string conditionJson, Visit visit)
    {
        try
        {
            using var doc = JsonDocument.Parse(conditionJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("skip_when", out var skipWhen))
                return false;

            var field = skipWhen.GetProperty("field").GetString();
            var op = skipWhen.GetProperty("operator").GetString();
            var expectedValue = skipWhen.GetProperty("value");

            var actualValue = GetFieldValue(field, visit);

            return op switch
            {
                "eq" => ValuesEqual(actualValue, expectedValue),
                "neq" => !ValuesEqual(actualValue, expectedValue),
                _ => false
            };
        }
        catch
        {
            return false;
        }
    }

    private static object? GetFieldValue(string? field, Visit visit)
    {
        return field switch
        {
            "visit.needs_medication" => visit.NeedsMedication,
            "visit.status" => visit.Status.ToString(),
            _ => null
        };
    }

    private static bool ValuesEqual(object? actual, JsonElement expected)
    {
        if (actual is null) return false;

        return expected.ValueKind switch
        {
            JsonValueKind.True => actual is bool b && b,
            JsonValueKind.False => actual is bool b2 && !b2,
            JsonValueKind.String => actual.ToString() == expected.GetString(),
            JsonValueKind.Number => actual is int i && i == expected.GetInt32(),
            _ => false
        };
    }
}
