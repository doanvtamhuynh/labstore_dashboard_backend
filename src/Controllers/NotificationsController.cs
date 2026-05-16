using backend.src.DTOs;
using backend.src.Helpers;
using backend.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[Authorize]
[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NotificationResponse>>>> List(CancellationToken cancellationToken)
    {
        return Ok(ApiResponse<IReadOnlyList<NotificationResponse>>.Ok(await _notificationService.ListAsync(cancellationToken)));
    }

    [HttpPatch("{id}/read")]
    public async Task<ActionResult<ApiResponse<NotificationResponse>>> MarkRead(string id, CancellationToken cancellationToken)
    {
        return Ok(ApiResponse<NotificationResponse>.Ok(await _notificationService.MarkReadAsync(id, cancellationToken), "Notification marked as read"));
    }

    [HttpPost("push")]
    public async Task<ActionResult<ApiResponse<NotificationResponse>>> Push(NotificationRequest request, CancellationToken cancellationToken)
    {
        return Ok(ApiResponse<NotificationResponse>.Ok(await _notificationService.PushAsync(request, cancellationToken), "Notification pushed"));
    }
}
