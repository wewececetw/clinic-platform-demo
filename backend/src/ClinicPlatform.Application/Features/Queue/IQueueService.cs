using ClinicPlatform.Application.Common;

namespace ClinicPlatform.Application.Features.Queue;

public interface IQueueService
{
    Task<Result<List<QueueEntryDto>>> GetQueueAsync(Guid clinicId, string queueType);
    Task<Result<List<QueueEntryDto>>> GetCalledAsync(Guid clinicId, string queueType);
    Task<Result<QueuePositionDto>> GetPositionAsync(Guid clinicId, Guid visitId);
    Task<Result<QueueEntryDto>> CallNextAsync(CallNextRequest request, Guid callerUserId);
    Task<Result> SkipAsync(Guid clinicId, Guid visitId, Guid callerUserId);
}
