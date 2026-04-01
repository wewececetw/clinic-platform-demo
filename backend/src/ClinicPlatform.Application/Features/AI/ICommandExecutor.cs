using ClinicPlatform.Application.Common;

namespace ClinicPlatform.Application.Features.AI;

public record CommandContext(
    Guid ClinicId,
    Guid UserId,
    string Role,
    string Action,
    Dictionary<string, object>? Params);

public record CommandExecutionResult(
    bool Success,
    string Message,
    Dictionary<string, object>? Data = null);

public interface ICommandExecutor
{
    string Action { get; }
    string[] AllowedRoles { get; }
    Task<CommandExecutionResult> ExecuteAsync(CommandContext context);
}
