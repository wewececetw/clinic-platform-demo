using ClinicPlatform.Application.Features.AI;
using ClinicPlatform.Application.Features.Queue;

namespace ClinicPlatform.Infrastructure.Services.AI.Executors;

public class QueryStatsExecutor(IQueueService queueService) : ICommandExecutor
{
    public string Action => "query_stats";
    public string[] AllowedRoles => ["Nurse", "Doctor", "Admin"];

    public async Task<CommandExecutionResult> ExecuteAsync(CommandContext context)
    {
        // 目前先用佇列資料做基本統計，未來可接更完整的 StatsService
        var waitingResult = await queueService.GetQueueAsync(context.ClinicId, "waiting");
        var waitingCount = waitingResult.Success ? waitingResult.Data!.Count : 0;

        return new CommandExecutionResult(true, $"今日候診中：{waitingCount} 位",
            new Dictionary<string, object>
            {
                ["waitingCount"] = waitingCount
            });
    }
}
