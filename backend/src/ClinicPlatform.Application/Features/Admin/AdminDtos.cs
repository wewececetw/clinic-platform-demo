namespace ClinicPlatform.Application.Features.Admin;

public record WorkflowDefinitionDto(Guid Id, string Name, string? Description, bool IsDefault, bool IsActive, List<WorkflowStepDto> Steps);
public record WorkflowStepDto(Guid Id, string StepCode, string DisplayName, int StepOrder, string? RequiredRole, bool IsSkippable, bool AutoAdvance);
public record CreateWorkflowRequest(Guid ClinicId, string Name, string? Description, bool IsDefault, List<CreateWorkflowStepRequest> Steps);
public record CreateWorkflowStepRequest(string StepCode, string DisplayName, int StepOrder, string? RequiredRole, bool IsSkippable, bool AutoAdvance);
public record ClinicSettingsDto(Guid ClinicId, List<string> AllowedCheckinMethods, string? BusinessHoursStart, string? BusinessHoursEnd);
