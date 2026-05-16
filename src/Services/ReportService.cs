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

    public async Task<byte[]> ExportAsync(string reportName, CancellationToken cancellationToken)
    {
        var normalized = reportName.ToLowerInvariant();
        var csv = normalized switch
        {
            "revenue" => ToCsv(await GetRevenueAsync(new DateRangeQuery(null, null), cancellationToken)),
            "products" => ToCsv(await GetProductsAsync(cancellationToken)),
            "inventory" => ToCsv(await GetInventoryAsync(cancellationToken)),
            "customers" => ToCsv(await GetCustomersAsync(cancellationToken)),
            "affiliate" => ToCsv(await GetAffiliateAsync(cancellationToken)),
            _ => $"report,generatedAtUtc{Environment.NewLine}{reportName},{DateTime.UtcNow:O}{Environment.NewLine}"
        };
        return Encoding.UTF8.GetBytes(csv);
    }

    private static string ToCsv(RevenueReportResponse report)
    {
        return $"revenue,orders,fromDate,toDate{Environment.NewLine}{report.Revenue},{report.Orders},{report.FromDate:O},{report.ToDate:O}{Environment.NewLine}";
    }

    private static string ToCsv(IReadOnlyList<TopProductResponse> rows)
    {
        var builder = new StringBuilder("productId,name,quantitySold,revenue").AppendLine();
        foreach (var row in rows)
        {
            builder.AppendLine($"{row.ProductId},{Escape(row.Name)},{row.QuantitySold},{row.Revenue}");
        }
        return builder.ToString();
    }

    private static string ToCsv(InventoryReportResponse report)
    {
        return $"lowStock,outOfStock{Environment.NewLine}{report.LowStock},{report.OutOfStock}{Environment.NewLine}";
    }

    private static string ToCsv(CustomerBehaviorReportResponse report)
    {
        return $"customers,orders,averageOrdersPerCustomer{Environment.NewLine}{report.Customers},{report.Orders},{report.AverageOrdersPerCustomer}{Environment.NewLine}";
    }

    private static string ToCsv(AffiliateReportResponse report)
    {
        return $"partners,estimatedCommission{Environment.NewLine}{report.Partners},{report.EstimatedCommission}{Environment.NewLine}";
    }

    private static string Escape(string value)
    {
        return value.Contains(',') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    }
}
