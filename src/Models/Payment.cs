using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.src.Models;

public enum PaymentTransactionStatus
{
    Pending,
    Succeeded,
    Failed,
    Refunded
}

public sealed class Payment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string OrderId { get; set; } = string.Empty;
    public string TransactionCode { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public PaymentTransactionStatus Status { get; set; } = PaymentTransactionStatus.Pending;
    public decimal Amount { get; set; }
    public decimal RefundedAmount { get; set; }
    public string? ProviderPayload { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
