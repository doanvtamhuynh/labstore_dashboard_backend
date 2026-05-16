using backend.src.DTOs;
using backend.src.Helpers;

namespace backend.src.Services;

public interface IOrderService
{
    Task<(IReadOnlyList<OrderResponse> Items, PaginationMetadata Pagination)> ListAsync(OrderQuery query, CancellationToken cancellationToken);
    Task<OrderResponse> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<OrderResponse> UpdateStatusAsync(string id, UpdateOrderStatusRequest request, string? changedBy, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrderHistoryResponse>> GetHistoryAsync(string id, CancellationToken cancellationToken);
    Task<byte[]> CreateInvoicePdfAsync(string id, CancellationToken cancellationToken);
    Task<string> ExportCsvAsync(OrderQuery query, CancellationToken cancellationToken);
}
