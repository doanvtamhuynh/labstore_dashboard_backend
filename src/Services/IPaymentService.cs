using backend.src.DTOs;
using backend.src.Helpers;

namespace backend.src.Services;

public interface IPaymentService
{
    Task<(IReadOnlyList<PaymentResponse> Items, PaginationMetadata Pagination)> ListAsync(PaymentQuery query, CancellationToken cancellationToken);
    Task<PaymentResponse> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<PaymentResponse> RefundAsync(string id, RefundRequest request, CancellationToken cancellationToken);
    Task<PaymentReconciliationResponse> GetReconciliationAsync(CancellationToken cancellationToken);
}
