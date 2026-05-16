using Microsoft.AspNetCore.SignalR;

namespace backend.src.Hubs;

public sealed class NotificationsHub : Hub
{
    public Task JoinAdminChannel()
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, "admins");
    }
}
