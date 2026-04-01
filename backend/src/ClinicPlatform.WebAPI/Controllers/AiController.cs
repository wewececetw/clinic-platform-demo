using ClinicPlatform.Application.Features.AI;
using ClinicPlatform.Infrastructure.Services.AI;
using Microsoft.AspNetCore.Mvc;

namespace ClinicPlatform.WebAPI.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController(IAiService aiService, CommandRouter commandRouter) : ControllerBase
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

    [HttpPost("command/execute")]
    public async Task<IActionResult> ExecuteCommand([FromBody] ExecuteCommandRequest request)
    {
        var context = new CommandContext(
            request.ClinicId,
            request.UserId,
            request.Role,
            request.Action,
            request.Params);

        var result = await commandRouter.RouteAsync(context);
        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }
}
