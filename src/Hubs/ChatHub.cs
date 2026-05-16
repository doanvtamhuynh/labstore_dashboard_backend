using Microsoft.AspNetCore.SignalR;

namespace backend.src.Hubs;

public sealed class ChatHub : Hub
{
    public Task JoinTicket(string ticketId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, $"ticket:{ticketId}");
    }

    public Task SendTicketMessage(string ticketId, string sender, string message)
    {
        return Clients.Group($"ticket:{ticketId}").SendAsync("ticketMessageReceived", new
        {
            ticketId,
            sender,
            message,
            sentAtUtc = DateTime.UtcNow
        });
    }
}
