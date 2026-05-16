using backend.src.DTOs;
using backend.src.Helpers;

namespace backend.src.Services;

public interface ICustomerService
{
    Task<(IReadOnlyList<CustomerResponse> Items, PaginationMetadata Pagination)> ListAsync(CustomerQuery query, CancellationToken cancellationToken);
    Task<CustomerDetailResponse> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<CustomerResponse> UpdateStatusAsync(string id, CustomerStatusRequest request, CancellationToken cancellationToken);
    Task<CustomerResponse> UpdateSegmentAsync(string id, CustomerSegmentRequest request, CancellationToken cancellationToken);
    Task<CustomerResponse> AddNoteAsync(string id, CustomerNoteRequest request, string? createdBy, CancellationToken cancellationToken);
    Task<CustomerResponse> UpdateLoyaltyAsync(string id, CustomerLoyaltyRequest request, CancellationToken cancellationToken);
}
