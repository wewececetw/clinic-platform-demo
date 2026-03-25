namespace ClinicPlatform.Application.Features.Prescription;

public record CreatePrescriptionRequest(Guid ClinicId, Guid VisitId, List<PrescriptionItemRequest> Items, string? Notes);
public record PrescriptionItemRequest(Guid MedicationId, string Dosage, string Frequency, int DurationDays, int Quantity, string? Instructions);
public record PrescriptionDto(Guid Id, Guid VisitId, string Status, string DoctorName, List<PrescriptionItemDto> Items, DateTime PrescribedAt);
public record PrescriptionItemDto(string MedicationName, string Dosage, string Frequency, int DurationDays, int Quantity, string? Instructions);
