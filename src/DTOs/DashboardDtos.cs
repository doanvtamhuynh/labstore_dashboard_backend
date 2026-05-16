namespace backend.src.DTOs;

public sealed record DashboardSummaryResponse(
    decimal Revenue,
    long Orders,
    long NewCustomers,
    long LowStockProducts);

public sealed record RevenueChartPoint(string Label, decimal Revenue, long Orders);

public sealed record TopProductResponse(string ProductId, string Name, long QuantitySold, decimal Revenue);

public sealed record GeoOrderResponse(string Province, long Orders);

public sealed record DashboardKpiResponse(decimal ConversionRate, decimal AverageOrderValue);
