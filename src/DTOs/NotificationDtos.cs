using backend.src.Models;

namespace backend.src.DTOs;

public sealed record NotificationRequest(string Title, string Message, NotificationAudience Audience, string? RecipientId);

public sealed record NotificationResponse(string Id, string Title, string Message, NotificationAudience Audience, string? RecipientId, bool IsRead, DateTime CreatedAtUtc, DateTime? ReadAtUtc);
