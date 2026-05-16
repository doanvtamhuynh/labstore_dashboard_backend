using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.src.Models;

public enum NotificationAudience
{
    Admin,
    Customer
}

public sealed class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationAudience Audience { get; set; } = NotificationAudience.Admin;
    public string? RecipientId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAtUtc { get; set; }
}
