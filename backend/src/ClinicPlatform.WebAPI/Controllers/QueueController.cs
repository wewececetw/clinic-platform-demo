using ClinicPlatform.Application.Features.Queue;
using Microsoft.AspNetCore.Mvc;

namespace ClinicPlatform.WebAPI.Controllers;

[ApiController]
[Route("api/queue")]
public class QueueController : ControllerBase
{
    private readonly IQueueService _queueService;

    private Guid CurrentUserId => Guid.Parse("99999999-0000-0000-0000-000000000001");

    public QueueController(IQueueService queueService)
    {
        _queueService = queueService;
    }

    [HttpGet("{clinicId}")]
    public async Task<IActionResult> GetQueue(Guid clinicId, [FromQuery] string queueType = "Consulting")
    {
        var result = await _queueService.GetQueueAsync(clinicId, queueType);
        return result.Success ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpGet("{clinicId}/position/{visitId}")]
    public async Task<IActionResult> GetPosition(Guid clinicId, Guid visitId)
    {
        var result = await _queueService.GetPositionAsync(clinicId, visitId);
        return result.Success ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpPost("call-next")]
    public async Task<IActionResult> CallNext([FromBody] CallNextRequest request)
    {
        var result = await _queueService.CallNextAsync(request, CurrentUserId);
        return result.Success ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpPost("call-pickup/{visitId}")]
    public async Task<IActionResult> CallPickup(Guid visitId, [FromQuery] Guid clinicId)
    {
        var result = await _queueService.CallNextAsync(new CallNextRequest(clinicId, "Pharmacy", null), CurrentUserId);
        return result.Success ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpPost("{visitId}/skip")]
    public async Task<IActionResult> Skip(Guid visitId, [FromQuery] Guid clinicId)
    {
        var result = await _queueService.SkipAsync(clinicId, visitId, CurrentUserId);
        return result.Success ? Ok() : BadRequest(result.Error);
    }
}
