using ClinicPlatform.Application.Features.AI;
using ClinicPlatform.Application.Features.Queue;

namespace ClinicPlatform.Infrastructure.Services.AI.Executors;

public class SkipExecutor(IQueueService queueService) : ICommandExecutor
{
    public string Action => "skip";
    public string[] AllowedRoles => ["Nurse"];

    public async Task<CommandExecutionResult> ExecuteAsync(CommandContext context)
    {
        var queueNumber = context.Params?.GetValueOrDefault("queueNumber");
        if (queueNumber is null)
            return new CommandExecutionResult(false, "請指定要過號的號碼");

        var num = Convert.ToInt32(queueNumber);

        // 先查佇列找到對應的 visitId
        var queueResult = await queueService.GetQueueAsync(context.ClinicId, "waiting");
        if (!queueResult.Success)
            return new CommandExecutionResult(false, queueResult.Error ?? "查詢佇列失敗");

        var entry = queueResult.Data!.FirstOrDefault(e => e.QueueNumber == num);
        if (entry is null)
            return new CommandExecutionResult(false, $"查無 {num} 號候診病患");

        var result = await queueService.SkipAsync(context.ClinicId, entry.VisitId, context.UserId);
        if (!result.Success)
            return new CommandExecutionResult(false, result.Error ?? "過號失敗");

        return new CommandExecutionResult(true, $"已將 {num} 號 {entry.PatientName} 標記為過號");
    }
}
