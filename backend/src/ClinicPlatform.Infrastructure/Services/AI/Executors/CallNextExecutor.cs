using ClinicPlatform.Application.Features.AI;
using ClinicPlatform.Application.Features.Queue;

namespace ClinicPlatform.Infrastructure.Services.AI.Executors;

public class CallNextExecutor(IQueueService queueService) : ICommandExecutor
{
    public string Action => "call_next";
    public string[] AllowedRoles => ["Nurse"];

    public async Task<CommandExecutionResult> ExecuteAsync(CommandContext context)
    {
        var queueType = context.Params?.GetValueOrDefault("queueType")?.ToString() ?? "waiting";
        var request = new CallNextRequest(context.ClinicId, queueType, null);
        var result = await queueService.CallNextAsync(request, context.UserId);

        if (!result.Success)
            return new CommandExecutionResult(false, result.Error ?? "叫號失敗");

        var entry = result.Data!;
        return new CommandExecutionResult(true, $"已叫號：{entry.QueueNumber} 號 {entry.PatientName}",
            new Dictionary<string, object>
            {
                ["queueNumber"] = entry.QueueNumber,
                ["patientName"] = entry.PatientName
            });
    }
}
