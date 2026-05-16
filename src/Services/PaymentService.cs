using backend.src.DTOs;
using backend.src.Helpers;
using backend.src.Models;
using backend.src.Repositories;

namespace backend.src.Services;

public sealed class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _payments;

    public PaymentService(IPaymentRepository payments)
    {
        _payments = payments;
    }

    public async Task<(IReadOnlyList<PaymentResponse> Items, PaginationMetadata Pagination)> ListAsync(PaymentQuery query, CancellationToken cancellationToken)
    {
        var (items, total) = await _payments.ListAsync(query, cancellationToken);
        var page = Math.Max(query.Page, 1);
        var limit = Math.Clamp(query.Limit, 1, 100);
        return (items.Select(ToResponse).ToList(), new PaginationMetadata(page, limit, total));
    }

    public async Task<PaymentResponse> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return ToResponse(await GetPaymentAsync(id, cancellationToken));
    }

    public async Task<PaymentResponse> RefundAsync(string id, RefundRequest request, CancellationToken cancellationToken)
    {
        var payment = await GetPaymentAsync(id, cancellationToken);
        if (request.Amount <= 0 || request.Amount > payment.Amount - payment.RefundedAmount)
        {
            throw new InvalidOperationException("Refund amount is invalid");
        }

        payment.RefundedAmount += request.Amount;
        if (payment.RefundedAmount >= payment.Amount)
        {
            payment.Status = PaymentTransactionStatus.Refunded;
        }

        await _payments.UpdateAsync(payment, cancellationToken);
        return ToResponse(payment);
    }

    public async Task<PaymentReconciliationResponse> GetReconciliationAsync(CancellationToken cancellationToken)
    {
        var payments = await _payments.ListSucceededAsync(cancellationToken);
        var gross = payments.Sum(payment => payment.Amount);
        var refunded = payments.Sum(payment => payment.RefundedAmount);
        return new PaymentReconciliationResponse(gross, refunded, gross - refunded, payments.Count);
    }

    private async Task<Payment> GetPaymentAsync(string id, CancellationToken cancellationToken)
    {
        return await _payments.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Payment not found");
    }

    private static PaymentResponse ToResponse(Payment payment)
    {
        return new PaymentResponse(payment.Id!, payment.OrderId, payment.TransactionCode, payment.Method, payment.Status, payment.Amount, payment.RefundedAmount, payment.CreatedAtUtc, payment.UpdatedAtUtc);
    }
}
