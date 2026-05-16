using backend.src.DTOs;

namespace backend.src.Services;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationResponse>> ListAsync(CancellationToken cancellationToken);
    Task<NotificationResponse> MarkReadAsync(string id, CancellationToken cancellationToken);
    Task<NotificationResponse> PushAsync(NotificationRequest request, CancellationToken cancellationToken);
}
