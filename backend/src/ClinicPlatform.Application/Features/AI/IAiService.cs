using ClinicPlatform.Application.Common;

namespace ClinicPlatform.Application.Features.AI;

public interface IAiService
{
    Task<Result<TriageResponse>> TriageAsync(TriageRequest request);
    Task<Result<CommandResponse>> CommandAsync(CommandRequest request);
}
