using ClinicPlatform.Application.Features.AI;
using ClinicPlatform.Application.Features.Visit;

namespace ClinicPlatform.Infrastructure.Services.AI.Executors;

public class CompleteConsultExecutor(IVisitService visitService) : ICommandExecutor
{
    public string Action => "complete_consult";
    public string[] AllowedRoles => ["Doctor"];

    public async Task<CommandExecutionResult> ExecuteAsync(CommandContext context)
    {
        var visitIdStr = context.Params?.GetValueOrDefault("visitId")?.ToString();
        if (visitIdStr is null || !Guid.TryParse(visitIdStr, out var visitId))
            return new CommandExecutionResult(false, "請先選擇看診中的病患");

        var needsMedication = context.Params?.GetValueOrDefault("needsMedication") is true;

        var request = new CompleteConsultRequest(context.ClinicId, visitId, needsMedication);
        var result = await visitService.CompleteConsultAsync(request, context.UserId);

        if (!result.Success)
            return new CommandExecutionResult(false, result.Error ?? "完成看診失敗");

        var msg = needsMedication ? "看診完成，病患已轉至藥局等候取藥" : "看診完成";
        return new CommandExecutionResult(true, msg);
    }
}
