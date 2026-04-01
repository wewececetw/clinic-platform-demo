using ClinicPlatform.Application.Features.AI;
using ClinicPlatform.Application.Features.Queue;

namespace ClinicPlatform.Infrastructure.Services.AI.Executors;

public class QueryQueueExecutor(IQueueService queueService) : ICommandExecutor
{
    public string Action => "query_queue";
    public string[] AllowedRoles => ["Nurse", "Doctor", "Admin"];

    public async Task<CommandExecutionResult> ExecuteAsync(CommandContext context)
    {
        var result = await queueService.GetQueueAsync(context.ClinicId, "waiting");
        if (!result.Success)
            return new CommandExecutionResult(false, result.Error ?? "查詢失敗");

        var queue = result.Data!;
        var count = queue.Count;
        var message = count == 0
            ? "目前沒有候診病患"
            : $"目前有 {count} 位候診病患";

        return new CommandExecutionResult(true, message,
            new Dictionary<string, object>
            {
                ["totalWaiting"] = count,
                ["entries"] = queue.Select(e => new { e.QueueNumber, e.PatientName, e.Priority }).ToList()
            });
    }
}
