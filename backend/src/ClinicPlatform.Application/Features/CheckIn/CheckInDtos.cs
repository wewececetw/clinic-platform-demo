namespace ClinicPlatform.Application.Features.CheckIn;

public record SendOtpRequest(Guid ClinicId, string Phone);
public record VerifyOtpRequest(Guid ClinicId, string Phone, string OtpCode);
public record QrCodeCheckInRequest(Guid ClinicId, string QrCodeToken);
public record ManualCheckInRequest(Guid ClinicId, string? Phone, string? FullName, Guid? DepartmentId, Guid? DoctorId);
public record CheckInResponse(Guid VisitId, int QueueNumber, string CurrentStep);
