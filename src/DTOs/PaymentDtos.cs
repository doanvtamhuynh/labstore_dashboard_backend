using backend.src.Models;

namespace backend.src.DTOs;

public sealed record PaymentQuery(string? Method, PaymentTransactionStatus? Status, DateTime? FromDate, DateTime? ToDate, int Page = 1, int Limit = 20);

public sealed record RefundRequest(decimal Amount, string? Reason);

public sealed record PaymentResponse(
    string Id,
    string OrderId,
    string TransactionCode,
    string Method,
    PaymentTransactionStatus Status,
    decimal Amount,
    decimal RefundedAmount,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record PaymentReconciliationResponse(decimal GrossRevenue, decimal RefundedAmount, decimal NetRevenue, long Transactions);
