using ClinicPlatform.Application.Common;

namespace ClinicPlatform.Application.Features.Workflow;

public interface IWorkflowEngine
{
    Task<Result> AdvanceAsync(Guid clinicId, Guid visitId, Guid? triggeredByUserId);
}
