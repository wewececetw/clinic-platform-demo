using ClinicPlatform.Application.Common;

namespace ClinicPlatform.Application.Features.CheckIn;

public interface ICheckInService
{
    Task<Result> SendOtpAsync(SendOtpRequest request);
    Task<Result<CheckInResponse>> VerifyOtpAsync(VerifyOtpRequest request);
    Task<Result<CheckInResponse>> QrCodeCheckInAsync(QrCodeCheckInRequest request);
    Task<Result<CheckInResponse>> ManualCheckInAsync(ManualCheckInRequest request, Guid nurseUserId);
}
