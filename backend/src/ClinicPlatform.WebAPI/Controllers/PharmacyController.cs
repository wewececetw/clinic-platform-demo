using ClinicPlatform.Application.Features.Prescription;
using Microsoft.AspNetCore.Mvc;

namespace ClinicPlatform.WebAPI.Controllers;

[ApiController]
[Route("api/pharmacy")]
public class PharmacyController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;

    private Guid CurrentUserId => Guid.Parse("99999999-0000-0000-0000-000000000001");

    public PharmacyController(IPrescriptionService prescriptionService)
    {
        _prescriptionService = prescriptionService;
    }

    [HttpGet("queue")]
    public async Task<IActionResult> GetPharmacyQueue([FromQuery] Guid clinicId)
    {
        var result = await _prescriptionService.GetPharmacyQueueAsync(clinicId);
        return result.Success ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpPost("prescriptions/{prescriptionId}/start-dispense")]
    public async Task<IActionResult> StartDispense(Guid prescriptionId, [FromQuery] Guid clinicId)
    {
        var result = await _prescriptionService.StartDispenseAsync(prescriptionId, CurrentUserId, clinicId);
        return result.Success ? Ok() : BadRequest(result.Error);
    }

    [HttpPost("prescriptions/{prescriptionId}/complete-dispense")]
    public async Task<IActionResult> CompleteDispense(Guid prescriptionId, [FromQuery] Guid clinicId)
    {
        var result = await _prescriptionService.CompleteDispenseAsync(prescriptionId, CurrentUserId, clinicId);
        return result.Success ? Ok() : BadRequest(result.Error);
    }
}
