using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.src.Models;

public enum CustomerStatus
{
    Active,
    Locked
}

public sealed class Customer
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public CustomerStatus Status { get; set; } = CustomerStatus.Active;
    public string Segment { get; set; } = "Regular";
    public int LoyaltyPoints { get; set; }
    public List<CustomerNote> Notes { get; set; } = [];
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class CustomerNote
{
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string Content { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
