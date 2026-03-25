using ClinicPlatform.Application.Features.CheckIn;
using Microsoft.AspNetCore.Mvc;

namespace ClinicPlatform.WebAPI.Controllers;

[ApiController]
[Route("api/checkin")]
public class CheckInController : ControllerBase
{
    private readonly ICheckInService _checkInService;

    private Guid CurrentUserId => Guid.Parse("99999999-0000-0000-0000-000000000001");

    public CheckInController(ICheckInService checkInService)
    {
        _checkInService = checkInService;
    }

    [HttpPost("otp/send")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        var result = await _checkInService.SendOtpAsync(request);
        return result.Success ? Ok() : BadRequest(result.Error);
    }

    [HttpPost("otp/verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var result = await _checkInService.VerifyOtpAsync(request);
        return result.Success ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpPost("qrcode")]
    public async Task<IActionResult> QrCodeCheckIn([FromBody] QrCodeCheckInRequest request)
    {
        var result = await _checkInService.QrCodeCheckInAsync(request);
        return result.Success ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpPost("manual")]
    public async Task<IActionResult> ManualCheckIn([FromBody] ManualCheckInRequest request)
    {
        var result = await _checkInService.ManualCheckInAsync(request, CurrentUserId);
        return result.Success ? Ok(result.Data) : BadRequest(result.Error);
    }
}
