using ClinicPlatform.Application.Features.Notifications;
using ClinicPlatform.WebAPI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ClinicPlatform.WebAPI.Notifications;

/// <summary>
/// SignalR 實作的通知推播器。Fire-and-forget + try-catch：推播失敗不影響業務交易。
/// </summary>
public class SignalRNotificationPublisher : INotificationPublisher
{
    private readonly IHubContext<VisitHub> _hub;
    private readonly ILogger<SignalRNotificationPublisher> _logger;

    public SignalRNotificationPublisher(
        IHubContext<VisitHub> hub,
        ILogger<SignalRNotificationPublisher> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task PublishQueueUpdatedAsync(Guid clinicId, string queueType, string changeType)
    {
        try
        {
            var payload = new
            {
                clinicId,
                queueType,
                changeType,
                timestamp = DateTime.UtcNow
            };
            await _hub.Clients.Group(GroupNames.ClinicGroup(clinicId))
                .SendAsync("QueueUpdated", payload);
            _logger.LogInformation("推播 QueueUpdated：clinic={ClinicId}, queueType={QueueType}, changeType={ChangeType}",
                clinicId, queueType, changeType);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "推播 QueueUpdated 失敗（已忽略）：clinic={ClinicId}", clinicId);
        }
    }

    public async Task PublishVisitStepChangedAsync(Guid visitId, string newStep, string stepName)
    {
        try
        {
            var payload = new
            {
                visitId,
                newStep,
                stepName,
                timestamp = DateTime.UtcNow
            };
            await _hub.Clients.Group(GroupNames.VisitGroup(visitId))
                .SendAsync("VisitStepChanged", payload);
            _logger.LogInformation("推播 VisitStepChanged：visit={VisitId}, step={Step}", visitId, newStep);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "推播 VisitStepChanged 失敗（已忽略）：visit={VisitId}", visitId);
        }
    }

    public async Task PublishPatientCalledAsync(Guid visitId, string roomNumber)
    {
        try
        {
            var payload = new
            {
                visitId,
                roomNumber,
                calledAt = DateTime.UtcNow
            };
            await _hub.Clients.Group(GroupNames.VisitGroup(visitId))
                .SendAsync("PatientCalled", payload);
            _logger.LogInformation("推播 PatientCalled：visit={VisitId}, room={Room}", visitId, roomNumber);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "推播 PatientCalled 失敗（已忽略）：visit={VisitId}", visitId);
        }
    }
}
