using backend.src.DTOs;
using backend.src.Helpers;

namespace backend.src.Services;

public interface IProductService
{
    Task<(IReadOnlyList<ProductResponse> Items, PaginationMetadata Pagination)> ListAsync(ProductQuery query, CancellationToken cancellationToken);
    Task<ProductResponse> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<ProductResponse> CreateAsync(ProductRequest request, CancellationToken cancellationToken);
    Task<ProductResponse> UpdateAsync(string id, ProductRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(string id, CancellationToken cancellationToken);
    Task<ProductResponse> ChangeStatusAsync(string id, ProductStatusRequest request, CancellationToken cancellationToken);
    Task<ProductResponse> AddImageAsync(string id, ProductImageRequest request, CancellationToken cancellationToken);
    Task DeleteImageAsync(string id, string imageId, CancellationToken cancellationToken);
    Task<ProductImportResult> ImportCsvAsync(IFormFile file, CancellationToken cancellationToken);
    Task<string> ExportCsvAsync(ProductQuery query, CancellationToken cancellationToken);
}
