using Microsoft.AspNetCore.SignalR;

namespace ClinicPlatform.WebAPI.Hubs;

public class VisitHub : Hub
{
    // Clients join a group by clinic ID to receive updates for their clinic only
    public async Task JoinClinic(string clinicId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"clinic_{clinicId}");
    }

    public async Task LeaveClinic(string clinicId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"clinic_{clinicId}");
    }

    // Clients join a visit-specific group for individual visit updates
    public async Task JoinVisit(string visitId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"visit_{visitId}");
    }

    public async Task LeaveVisit(string visitId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"visit_{visitId}");
    }
}
