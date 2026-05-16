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

    public async Task<(byte[] Bytes, string ContentType, string FileName)> ExportAsync(string reportName, string? format, CancellationToken cancellationToken)
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

        if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Take(35).ToList();
            lines.Insert(0, $"Labstore {normalized} report");
            return (BuildSimplePdf(lines), "application/pdf", $"{normalized}-report.pdf");
        }

        return (Encoding.UTF8.GetBytes(csv), "text/csv", $"{normalized}-report.csv");
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

    private static byte[] BuildSimplePdf(IReadOnlyList<string> lines)
    {
        var stream = string.Join(Environment.NewLine, lines.Select((line, index) => $"BT /F1 11 Tf 48 {760 - (index * 18)} Td ({EscapePdf(line)}) Tj ET"));
        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}\nendstream"
        };
        var builder = new StringBuilder("%PDF-1.4\n");
        var offsets = new List<int> { 0 };
        foreach (var (body, index) in objects.Select((body, index) => (body, index)))
        {
            offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString()));
            builder.Append(index + 1).Append(" 0 obj\n").Append(body).Append("\nendobj\n");
        }
        var xrefOffset = Encoding.ASCII.GetByteCount(builder.ToString());
        builder.Append("xref\n0 ").Append(objects.Length + 1).Append('\n');
        builder.Append("0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1))
        {
            builder.Append(offset.ToString("D10", System.Globalization.CultureInfo.InvariantCulture)).Append(" 00000 n \n");
        }
        builder.Append("trailer\n<< /Size ").Append(objects.Length + 1).Append(" /Root 1 0 R >>\nstartxref\n").Append(xrefOffset).Append("\n%%EOF");
        return Encoding.ASCII.GetBytes(builder.ToString());
    }

    private static string EscapePdf(string value)
    {
        return value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
    }
}
