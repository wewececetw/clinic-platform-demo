using ClinicPlatform.Application.Features.Queue;
using ClinicPlatform.Application.Features.Visit;
using ClinicPlatform.Application.Features.Prescription;
using Microsoft.AspNetCore.Mvc;

namespace ClinicPlatform.WebAPI.Controllers;

[ApiController]
[Route("api/doctor")]
public class DoctorController : ControllerBase
{
    private readonly IVisitService _visitService;
    private readonly IPrescriptionService _prescriptionService;
    private readonly IQueueService _queueService;

    private Guid CurrentUserId => Guid.Parse("99999999-0000-0000-0000-000000000001");

    public DoctorController(IVisitService visitService, IPrescriptionService prescriptionService, IQueueService queueService)
    {
        _visitService = visitService;
        _prescriptionService = prescriptionService;
        _queueService = queueService;
    }

    [HttpGet("queue")]
    public async Task<IActionResult> GetDoctorQueue([FromQuery] Guid clinicId)
    {
        // 醫師頁需看到已被叫號（Called）等待開始看診的病患
        var result = await _queueService.GetCalledAsync(clinicId, "Consulting");
        return result.Success ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpPost("visits/{visitId}/start-consult")]
    public async Task<IActionResult> StartConsult(Guid visitId, [FromQuery] Guid clinicId)
    {
        var result = await _visitService.StartConsultAsync(new StartConsultRequest(clinicId, visitId), CurrentUserId);
        return result.Success ? Ok() : BadRequest(result.Error);
    }

    [HttpPost("visits/{visitId}/prescriptions")]
    public async Task<IActionResult> CreatePrescription(Guid visitId, [FromBody] CreatePrescriptionRequest request)
    {
        var result = await _prescriptionService.CreateAsync(request, CurrentUserId);
        return result.Success ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpPost("visits/{visitId}/complete-consult")]
    public async Task<IActionResult> CompleteConsult(Guid visitId, [FromBody] CompleteConsultRequest request)
    {
        var result = await _visitService.CompleteConsultAsync(request, CurrentUserId);
        return result.Success ? Ok() : BadRequest(result.Error);
    }
}
