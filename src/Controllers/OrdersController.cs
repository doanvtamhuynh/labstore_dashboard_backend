using System.Security.Claims;
using System.Text;
using backend.src.DTOs;
using backend.src.Helpers;
using backend.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Get orders with status, date range, customer filter, and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OrderResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OrderResponse>>>> List([FromQuery] OrderQuery query, CancellationToken cancellationToken)
    {
        var (items, pagination) = await _orderService.ListAsync(query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<OrderResponse>>.Ok(items, pagination: pagination));
    }

    /// <summary>
    /// Get order detail by id.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> GetById(string id, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<OrderResponse>.Ok(result));
    }

    /// <summary>
    /// Update order status and append a history entry.
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> UpdateStatus(string id, UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var changedBy = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _orderService.UpdateStatusAsync(id, request, changedBy, cancellationToken);
        return Ok(ApiResponse<OrderResponse>.Ok(result, "Order status updated"));
    }

    /// <summary>
    /// Get order status change history.
    /// </summary>
    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OrderHistoryResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OrderHistoryResponse>>>> GetHistory(string id, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetHistoryAsync(id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<OrderHistoryResponse>>.Ok(result));
    }

    /// <summary>
    /// Export a simple invoice PDF file.
    /// </summary>
    [HttpGet("{id}/invoice")]
    [Produces("application/pdf")]
    public async Task<FileResult> GetInvoice(string id, CancellationToken cancellationToken)
    {
        var bytes = await _orderService.CreateInvoicePdfAsync(id, cancellationToken);
        return File(bytes, "application/pdf", $"invoice-{id}.pdf");
    }

    /// <summary>
    /// Export filtered orders to CSV.
    /// </summary>
    [HttpGet("export")]
    [Produces("text/csv")]
    public async Task<FileResult> Export([FromQuery] OrderQuery query, CancellationToken cancellationToken)
    {
        var csv = await _orderService.ExportCsvAsync(query, cancellationToken);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", "orders.csv");
    }
}
