using ClinicPlatform.Application.Features.AI;
using Microsoft.AspNetCore.Mvc;

namespace ClinicPlatform.WebAPI.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController(IAiService aiService) : ControllerBase
{
    [HttpPost("triage")]
    public async Task<IActionResult> Triage([FromBody] TriageRequest request)
    {
        var result = await aiService.TriageAsync(request);
        return result.Success
            ? Ok(result.Data)
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("command")]
    public async Task<IActionResult> Command([FromBody] CommandRequest request)
    {
        var result = await aiService.CommandAsync(request);
        return result.Success
            ? Ok(result.Data)
            : BadRequest(new { error = result.Error });
    }
}
