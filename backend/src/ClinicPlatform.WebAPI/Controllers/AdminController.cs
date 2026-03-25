using ClinicPlatform.Application.Features.Admin;
using Microsoft.AspNetCore.Mvc;

namespace ClinicPlatform.WebAPI.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    [HttpGet("workflows")]
    public IActionResult GetWorkflows([FromQuery] Guid clinicId)
    {
        return Ok();
    }

    [HttpPost("workflows")]
    public IActionResult CreateWorkflow([FromBody] CreateWorkflowRequest request)
    {
        return Ok();
    }

    [HttpGet("clinic/settings")]
    public IActionResult GetSettings([FromQuery] Guid clinicId)
    {
        return Ok();
    }

    [HttpPut("clinic/settings")]
    public IActionResult UpdateSettings([FromBody] ClinicSettingsDto settings)
    {
        return Ok();
    }
}
