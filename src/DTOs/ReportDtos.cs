namespace backend.src.DTOs;

public sealed record DateRangeQuery(DateTime? FromDate, DateTime? ToDate);

public sealed record RevenueReportResponse(decimal Revenue, long Orders, DateTime? FromDate, DateTime? ToDate);

public sealed record InventoryReportResponse(long LowStock, long OutOfStock);

public sealed record CustomerBehaviorReportResponse(long Customers, long Orders, decimal AverageOrdersPerCustomer);

public sealed record AffiliateReportResponse(long Partners, decimal EstimatedCommission);
