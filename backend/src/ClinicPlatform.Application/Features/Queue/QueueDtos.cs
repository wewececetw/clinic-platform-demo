namespace ClinicPlatform.Application.Features.Queue;

public record QueueEntryDto(Guid VisitId, int QueueNumber, string PatientName, int Priority, string Status, DateTime CheckedInAt);
public record QueuePositionDto(int Position, int QueueNumber, int TotalWaiting);
public record CallNextRequest(Guid ClinicId, string QueueType, Guid? RoomId);
