using backend.src.DTOs;

namespace backend.src.Services;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<RevenueChartPoint>> GetRevenueChartAsync(string period, CancellationToken cancellationToken);
    Task<IReadOnlyList<TopProductResponse>> GetTopProductsAsync(int limit, CancellationToken cancellationToken);
    Task<IReadOnlyList<GeoOrderResponse>> GetGeoOrdersAsync(CancellationToken cancellationToken);
    Task<DashboardKpiResponse> GetKpiAsync(CancellationToken cancellationToken);
}
