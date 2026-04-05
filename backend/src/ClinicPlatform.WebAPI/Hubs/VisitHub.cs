using Microsoft.AspNetCore.SignalR;

namespace ClinicPlatform.WebAPI.Hubs;

// TODO: 未來導入 JWT 後加上 [Authorize]，並從 Context.User claim 驗證 clinicId 與傳入參數一致
public class VisitHub : Hub
{
    private readonly ILogger<VisitHub> _logger;

    public VisitHub(ILogger<VisitHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("SignalR 連線建立：connectionId={ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is not null)
        {
            _logger.LogWarning(exception, "SignalR 連線異常中斷：connectionId={ConnectionId}", Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("SignalR 連線關閉：connectionId={ConnectionId}", Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    // === 診所群組（叫號台、看診室訂閱該診所全部佇列事件）===
    public async Task JoinClinicGroup(string clinicId)
    {
        if (!Guid.TryParse(clinicId, out var id))
        {
            _logger.LogWarning("JoinClinicGroup 傳入無效 clinicId={Input}", clinicId);
            return;
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupNames.ClinicGroup(id));
    }

    public async Task LeaveClinicGroup(string clinicId)
    {
        if (!Guid.TryParse(clinicId, out var id)) return;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupNames.ClinicGroup(id));
    }

    // === Visit 群組（病患訂閱自己的就診事件）===
    public async Task JoinVisitGroup(string visitId)
    {
        if (!Guid.TryParse(visitId, out var id))
        {
            _logger.LogWarning("JoinVisitGroup 傳入無效 visitId={Input}", visitId);
            return;
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupNames.VisitGroup(id));
    }

    public async Task LeaveVisitGroup(string visitId)
    {
        if (!Guid.TryParse(visitId, out var id)) return;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupNames.VisitGroup(id));
    }
}
