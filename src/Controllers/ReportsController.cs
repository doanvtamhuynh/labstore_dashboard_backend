using backend.src.DTOs;
using backend.src.Helpers;
using backend.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[Authorize]
[ApiController]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<ApiResponse<RevenueReportResponse>>> Revenue([FromQuery] DateRangeQuery query, CancellationToken cancellationToken) => Ok(ApiResponse<RevenueReportResponse>.Ok(await _reportService.GetRevenueAsync(query, cancellationToken)));

    [HttpGet("products")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TopProductResponse>>>> Products(CancellationToken cancellationToken) => Ok(ApiResponse<IReadOnlyList<TopProductResponse>>.Ok(await _reportService.GetProductsAsync(cancellationToken)));

    [HttpGet("inventory")]
    public async Task<ActionResult<ApiResponse<InventoryReportResponse>>> Inventory(CancellationToken cancellationToken) => Ok(ApiResponse<InventoryReportResponse>.Ok(await _reportService.GetInventoryAsync(cancellationToken)));

    [HttpGet("customers")]
    public async Task<ActionResult<ApiResponse<CustomerBehaviorReportResponse>>> Customers(CancellationToken cancellationToken) => Ok(ApiResponse<CustomerBehaviorReportResponse>.Ok(await _reportService.GetCustomersAsync(cancellationToken)));

    [HttpGet("affiliate")]
    public async Task<ActionResult<ApiResponse<AffiliateReportResponse>>> Affiliate(CancellationToken cancellationToken) => Ok(ApiResponse<AffiliateReportResponse>.Ok(await _reportService.GetAffiliateAsync(cancellationToken)));

    [HttpGet("{reportName}/export")]
    public async Task<FileResult> Export(string reportName, [FromQuery] string? format, CancellationToken cancellationToken)
    {
        var export = await _reportService.ExportAsync(reportName, format, cancellationToken);
        return File(export.Bytes, export.ContentType, export.FileName);
    }
}
