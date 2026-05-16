using backend.src.Models;

namespace backend.src.DTOs;

public sealed record CustomerQuery(string? Search, string? Segment, CustomerStatus? Status, int Page = 1, int Limit = 20);

public sealed record CustomerStatusRequest(CustomerStatus Status);

public sealed record CustomerSegmentRequest(string Segment);

public sealed record CustomerNoteRequest(string Content);

public sealed record CustomerLoyaltyRequest(int Points);

public sealed record CustomerNoteResponse(string Id, string Content, string? CreatedBy, DateTime CreatedAtUtc);

public sealed record CustomerResponse(
    string Id,
    string FullName,
    string Email,
    string? Phone,
    CustomerStatus Status,
    string Segment,
    int LoyaltyPoints,
    IReadOnlyList<CustomerNoteResponse> Notes,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record CustomerDetailResponse(CustomerResponse Customer, IReadOnlyList<OrderResponse> Orders);
