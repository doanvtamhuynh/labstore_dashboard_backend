using System.Security.Claims;
using backend.src.DTOs;
using backend.src.Helpers;
using backend.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[Authorize]
[ApiController]
[Route("api/customers")]
public sealed class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Get customers with search, segment/status filters, and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CustomerResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CustomerResponse>>>> List([FromQuery] CustomerQuery query, CancellationToken cancellationToken)
    {
        var (items, pagination) = await _customerService.ListAsync(query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CustomerResponse>>.Ok(items, pagination: pagination));
    }

    /// <summary>
    /// Get customer detail with recent purchase history.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerDetailResponse>>> GetById(string id, CancellationToken cancellationToken)
    {
        var result = await _customerService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<CustomerDetailResponse>.Ok(result));
    }

    /// <summary>
    /// Lock or unlock a customer account.
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> UpdateStatus(string id, CustomerStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _customerService.UpdateStatusAsync(id, request, cancellationToken);
        return Ok(ApiResponse<CustomerResponse>.Ok(result, "Customer status updated"));
    }

    /// <summary>
    /// Assign a customer segment.
    /// </summary>
    [HttpPut("{id}/segment")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> UpdateSegment(string id, CustomerSegmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _customerService.UpdateSegmentAsync(id, request, cancellationToken);
        return Ok(ApiResponse<CustomerResponse>.Ok(result, "Customer segment updated"));
    }

    /// <summary>
    /// Add a CRM note to a customer.
    /// </summary>
    [HttpPost("{id}/notes")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> AddNote(string id, CustomerNoteRequest request, CancellationToken cancellationToken)
    {
        var createdBy = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _customerService.AddNoteAsync(id, request, createdBy, cancellationToken);
        return Ok(ApiResponse<CustomerResponse>.Ok(result, "Customer note added"));
    }

    /// <summary>
    /// Update loyalty points for a customer.
    /// </summary>
    [HttpPut("{id}/loyalty")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> UpdateLoyalty(string id, CustomerLoyaltyRequest request, CancellationToken cancellationToken)
    {
        var result = await _customerService.UpdateLoyaltyAsync(id, request, cancellationToken);
        return Ok(ApiResponse<CustomerResponse>.Ok(result, "Customer loyalty updated"));
    }
}
