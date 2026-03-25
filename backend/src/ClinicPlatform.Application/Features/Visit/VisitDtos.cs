namespace ClinicPlatform.Application.Features.Visit;

public record VisitStatusDto(Guid VisitId, string CurrentStep, string CurrentStepDisplayName, int QueueNumber, string Status, bool NeedsMedication, DateTime CheckedInAt, DateTime? CompletedAt);
public record VisitEventDto(string FromStep, string ToStep, string TriggerType, string? TriggeredBy, DateTime CreatedAt);
public record StartConsultRequest(Guid ClinicId, Guid VisitId);
public record CompleteConsultRequest(Guid ClinicId, Guid VisitId, bool NeedsMedication);
