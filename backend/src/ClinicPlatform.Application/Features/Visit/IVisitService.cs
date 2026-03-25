using ClinicPlatform.Application.Common;

namespace ClinicPlatform.Application.Features.Visit;

public interface IVisitService
{
    Task<Result<VisitStatusDto>> GetStatusAsync(Guid clinicId, Guid visitId);
    Task<Result<List<VisitEventDto>>> GetEventsAsync(Guid clinicId, Guid visitId);
    Task<Result> StartConsultAsync(StartConsultRequest request, Guid doctorUserId);
    Task<Result> CompleteConsultAsync(CompleteConsultRequest request, Guid doctorUserId);
    Task<Result> AdvanceStepAsync(Guid clinicId, Guid visitId, Guid callerUserId);
}
