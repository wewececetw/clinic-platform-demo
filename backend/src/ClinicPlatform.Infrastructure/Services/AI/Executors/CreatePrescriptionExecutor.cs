using ClinicPlatform.Application.Features.AI;
using ClinicPlatform.Application.Features.Prescription;
using ClinicPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicPlatform.Infrastructure.Services.AI.Executors;

public class CreatePrescriptionExecutor(
    IPrescriptionService prescriptionService,
    ClinicDbContext dbContext) : ICommandExecutor
{
    public string Action => "create_prescription";
    public string[] AllowedRoles => ["Doctor"];

    public async Task<CommandExecutionResult> ExecuteAsync(CommandContext context)
    {
        var visitIdStr = context.Params?.GetValueOrDefault("visitId")?.ToString();
        if (visitIdStr is null || !Guid.TryParse(visitIdStr, out var visitId))
            return new CommandExecutionResult(false, "請先選擇看診中的病患");

        var drugName = context.Params?.GetValueOrDefault("drugName")?.ToString();
        if (string.IsNullOrWhiteSpace(drugName))
            return new CommandExecutionResult(false, "請指定藥品名稱");

        // 模糊匹配藥品
        var medication = await dbContext.Medications
            .Where(m => m.ClinicId == context.ClinicId && m.IsActive)
            .Where(m => m.Name.Contains(drugName))
            .FirstOrDefaultAsync();

        if (medication is null)
            return new CommandExecutionResult(false, $"找不到藥品「{drugName}」，請確認名稱");

        var dosage = context.Params?.GetValueOrDefault("dosage")?.ToString() ?? medication.DefaultDosage ?? "";
        var frequency = context.Params?.GetValueOrDefault("frequency")?.ToString() ?? "TID";
        var days = context.Params?.GetValueOrDefault("days") is { } d ? Convert.ToInt32(d) : 3;

        // 根據頻率計算總量
        var timesPerDay = frequency.ToUpper() switch
        {
            "QD" => 1,
            "BID" => 2,
            "TID" => 3,
            "QID" => 4,
            _ => 3
        };
        var quantity = timesPerDay * days;

        var request = new CreatePrescriptionRequest(
            context.ClinicId,
            visitId,
            [new PrescriptionItemRequest(medication.Id, dosage, frequency, days, quantity, null)],
            null);

        var result = await prescriptionService.CreateAsync(request, context.UserId);
        if (!result.Success)
            return new CommandExecutionResult(false, result.Error ?? "開立處方失敗");

        return new CommandExecutionResult(true,
            $"已開立處方：{medication.Name} {dosage} {frequency} {days}天（共{quantity}{medication.Unit}）",
            new Dictionary<string, object>
            {
                ["prescriptionId"] = result.Data!.Id,
                ["medicationName"] = medication.Name,
                ["dosage"] = dosage,
                ["frequency"] = frequency,
                ["days"] = days,
                ["quantity"] = quantity
            });
    }
}
