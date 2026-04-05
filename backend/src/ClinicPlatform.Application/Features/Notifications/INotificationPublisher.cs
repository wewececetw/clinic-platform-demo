namespace ClinicPlatform.Application.Features.Notifications;

/// <summary>
/// 即時通知推播介面。實作層負責透過 SignalR（或其他通道）將事件推送至對應群組。
/// 推播失敗不應中斷原始業務交易（fire-and-forget + log）。
/// </summary>
public interface INotificationPublisher
{
    /// <summary>
    /// 通知診所佇列有變動（報到、叫號、過號、看診開始/結束）。
    /// 目標群組：clinic_{clinicId}
    /// </summary>
    Task PublishQueueUpdatedAsync(Guid clinicId, string queueType, string changeType);

    /// <summary>
    /// 通知某個 Visit 的 workflow step 推進。
    /// 目標群組：visit_{visitId}
    /// </summary>
    Task PublishVisitStepChangedAsync(Guid visitId, string newStep, string stepName);

    /// <summary>
    /// 通知病患被叫號（顯示請至某診間）。
    /// 目標群組：visit_{visitId}
    /// </summary>
    Task PublishPatientCalledAsync(Guid visitId, string roomNumber);
}
