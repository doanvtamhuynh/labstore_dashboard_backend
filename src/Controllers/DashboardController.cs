using backend.src.DTOs;
using backend.src.Helpers;
using backend.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[Authorize]
[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Get revenue, order, customer, and low stock summary cards.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DashboardSummaryResponse>>> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetSummaryAsync(cancellationToken);
        return Ok(ApiResponse<DashboardSummaryResponse>.Ok(result));
    }

    /// <summary>
    /// Get revenue chart data grouped by day, week, month, or year.
    /// </summary>
    [HttpGet("revenue-chart")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RevenueChartPoint>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RevenueChartPoint>>>> GetRevenueChart([FromQuery] string period = "month", CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetRevenueChartAsync(period, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RevenueChartPoint>>.Ok(result));
    }

    /// <summary>
    /// Get top selling products from order items.
    /// </summary>
    [HttpGet("top-products")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TopProductResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TopProductResponse>>>> GetTopProducts([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetTopProductsAsync(limit, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TopProductResponse>>.Ok(result));
    }

    /// <summary>
    /// Get order distribution by province or city.
    /// </summary>
    [HttpGet("geo-orders")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GeoOrderResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GeoOrderResponse>>>> GetGeoOrders(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetGeoOrdersAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GeoOrderResponse>>.Ok(result));
    }

    /// <summary>
    /// Get conversion rate and average order value.
    /// </summary>
    [HttpGet("kpi")]
    [ProducesResponseType(typeof(ApiResponse<DashboardKpiResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DashboardKpiResponse>>> GetKpi(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetKpiAsync(cancellationToken);
        return Ok(ApiResponse<DashboardKpiResponse>.Ok(result));
    }
}
