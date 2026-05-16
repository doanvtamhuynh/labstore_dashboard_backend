using System.Text;
using backend.src.DTOs;
using backend.src.Repositories;

namespace backend.src.Services;

public sealed class ReportService : IReportService
{
    private readonly IReportRepository _reports;

    public ReportService(IReportRepository reports)
    {
        _reports = reports;
    }

    public Task<RevenueReportResponse> GetRevenueAsync(DateRangeQuery query, CancellationToken cancellationToken) => _reports.GetRevenueAsync(query, cancellationToken);
    public Task<IReadOnlyList<TopProductResponse>> GetProductsAsync(CancellationToken cancellationToken) => _reports.GetProductsAsync(cancellationToken);
    public Task<InventoryReportResponse> GetInventoryAsync(CancellationToken cancellationToken) => _reports.GetInventoryAsync(cancellationToken);
    public Task<CustomerBehaviorReportResponse> GetCustomersAsync(CancellationToken cancellationToken) => _reports.GetCustomersAsync(cancellationToken);
    public Task<AffiliateReportResponse> GetAffiliateAsync(CancellationToken cancellationToken) => _reports.GetAffiliateAsync(cancellationToken);

    public Task<byte[]> ExportAsync(string reportName, CancellationToken cancellationToken)
    {
        var csv = $"report,generatedAtUtc{Environment.NewLine}{reportName},{DateTime.UtcNow:O}{Environment.NewLine}";
        return Task.FromResult(Encoding.UTF8.GetBytes(csv));
    }
}
