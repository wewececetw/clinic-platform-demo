namespace ClinicPlatform.Application.Features.AI;

public record TriageRequest(Guid ClinicId, string Symptoms);

public record TriageResponse(
    string Department,
    Guid? DepartmentId,
    int Priority,
    int EstimatedWaitMinutes,
    string Reasoning);

public record CommandRequest(Guid ClinicId, Guid UserId, string Role, string Command);

public record CommandResponse(
    string Action,
    Dictionary<string, object>? Params,
    string Result,
    string Message);

public record ExecuteCommandRequest(
    Guid ClinicId,
    Guid UserId,
    string Role,
    string Action,
    Dictionary<string, object>? Params);
