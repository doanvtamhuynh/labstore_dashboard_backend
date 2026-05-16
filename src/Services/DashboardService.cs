using backend.src.DTOs;
using backend.src.Repositories;

namespace backend.src.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        return _dashboardRepository.GetSummaryAsync(cancellationToken);
    }

    public Task<IReadOnlyList<RevenueChartPoint>> GetRevenueChartAsync(string period, CancellationToken cancellationToken)
    {
        var allowedPeriods = new[] { "day", "week", "month", "year" };
        var normalizedPeriod = allowedPeriods.Contains(period.ToLowerInvariant()) ? period : "month";
        return _dashboardRepository.GetRevenueChartAsync(normalizedPeriod, cancellationToken);
    }

    public Task<IReadOnlyList<TopProductResponse>> GetTopProductsAsync(int limit, CancellationToken cancellationToken)
    {
        return _dashboardRepository.GetTopProductsAsync(limit, cancellationToken);
    }

    public Task<IReadOnlyList<GeoOrderResponse>> GetGeoOrdersAsync(CancellationToken cancellationToken)
    {
        return _dashboardRepository.GetGeoOrdersAsync(cancellationToken);
    }

    public Task<DashboardKpiResponse> GetKpiAsync(CancellationToken cancellationToken)
    {
        return _dashboardRepository.GetKpiAsync(cancellationToken);
    }
}
