using backend.src.DTOs;
using backend.src.Hubs;
using backend.src.Models;
using backend.src.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace backend.src.Services;

public sealed class NotificationService : INotificationService
{
    private readonly ICrudRepository<Notification> _notifications;
    private readonly IHubContext<NotificationsHub> _hubContext;

    public NotificationService(ICrudRepository<Notification> notifications, IHubContext<NotificationsHub> hubContext)
    {
        _notifications = notifications;
        _hubContext = hubContext;
    }

    public async Task<IReadOnlyList<NotificationResponse>> ListAsync(CancellationToken cancellationToken)
    {
        return (await _notifications.ListAsync(cancellationToken)).OrderByDescending(item => item.CreatedAtUtc).Select(ToResponse).ToList();
    }

    public async Task<NotificationResponse> MarkReadAsync(string id, CancellationToken cancellationToken)
    {
        var item = await _notifications.GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Notification not found");
        item.IsRead = true;
        item.ReadAtUtc = DateTime.UtcNow;
        await _notifications.UpdateAsync(id, item, cancellationToken);
        return ToResponse(item);
    }

    public async Task<NotificationResponse> PushAsync(NotificationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Message))
        {
            throw new InvalidOperationException("Notification title and message are required");
        }

        var item = new Notification
        {
            Title = request.Title.Trim(),
            Message = request.Message.Trim(),
            Audience = request.Audience,
            RecipientId = request.RecipientId
        };
        await _notifications.CreateAsync(item, cancellationToken);
        var response = ToResponse(item);
        await _hubContext.Clients.All.SendAsync("notificationReceived", response, cancellationToken);
        return response;
    }

    private static NotificationResponse ToResponse(Notification item)
    {
        return new NotificationResponse(item.Id!, item.Title, item.Message, item.Audience, item.RecipientId, item.IsRead, item.CreatedAtUtc, item.ReadAtUtc);
    }
}
