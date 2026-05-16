using backend.src.Models;

namespace backend.src.DTOs;

public sealed record ReviewQuery(int? Rating, string? ProductId, ReviewStatus? Status, int Page = 1, int Limit = 20);

public sealed record ReviewStatusRequest(ReviewStatus Status);

public sealed record ReviewReplyRequest(string Reply);

public sealed record ReviewResponse(
    string Id,
    string ProductId,
    string CustomerId,
    string CustomerName,
    int Rating,
    string Comment,
    ReviewStatus Status,
    bool IsFlagged,
    string? Reply,
    string? RepliedBy,
    DateTime? RepliedAtUtc,
    DateTime CreatedAtUtc);
