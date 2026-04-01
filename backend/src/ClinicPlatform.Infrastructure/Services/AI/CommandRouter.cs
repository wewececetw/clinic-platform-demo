using ClinicPlatform.Application.Features.AI;
using Microsoft.Extensions.Logging;

namespace ClinicPlatform.Infrastructure.Services.AI;

public class CommandRouter(
    IEnumerable<ICommandExecutor> executors,
    ILogger<CommandRouter> logger)
{
    private readonly Dictionary<string, ICommandExecutor> _executorMap =
        executors.ToDictionary(e => e.Action, e => e);

    public async Task<CommandExecutionResult> RouteAsync(CommandContext context)
    {
        if (!_executorMap.TryGetValue(context.Action, out var executor))
        {
            logger.LogWarning("未知的 action：{Action}", context.Action);
            return new CommandExecutionResult(false, $"不支援的指令：{context.Action}");
        }

        if (!executor.AllowedRoles.Contains(context.Role))
        {
            logger.LogWarning("角色 {Role} 無權執行 {Action}", context.Role, context.Action);
            return new CommandExecutionResult(false, "您沒有權限執行此操作");
        }

        try
        {
            return await executor.ExecuteAsync(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "執行 {Action} 時發生錯誤", context.Action);
            return new CommandExecutionResult(false, "指令執行失敗，請稍後再試");
        }
    }
}
