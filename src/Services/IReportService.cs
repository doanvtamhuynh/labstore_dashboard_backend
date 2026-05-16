using backend.src.DTOs;

namespace backend.src.Services;

public interface IReportService
{
    Task<RevenueReportResponse> GetRevenueAsync(DateRangeQuery query, CancellationToken cancellationToken);
    Task<IReadOnlyList<TopProductResponse>> GetProductsAsync(CancellationToken cancellationToken);
    Task<InventoryReportResponse> GetInventoryAsync(CancellationToken cancellationToken);
    Task<CustomerBehaviorReportResponse> GetCustomersAsync(CancellationToken cancellationToken);
    Task<AffiliateReportResponse> GetAffiliateAsync(CancellationToken cancellationToken);
    Task<(byte[] Bytes, string ContentType, string FileName)> ExportAsync(string reportName, string? format, CancellationToken cancellationToken);
}
