using backend.src.DTOs;
using backend.src.Helpers;
using backend.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[Authorize]
[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PaymentResponse>>>> List([FromQuery] PaymentQuery query, CancellationToken cancellationToken)
    {
        var (items, pagination) = await _paymentService.ListAsync(query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PaymentResponse>>.Ok(items, pagination: pagination));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PaymentResponse>>> GetById(string id, CancellationToken cancellationToken)
    {
        return Ok(ApiResponse<PaymentResponse>.Ok(await _paymentService.GetByIdAsync(id, cancellationToken)));
    }

    [HttpPost("{id}/refund")]
    public async Task<ActionResult<ApiResponse<PaymentResponse>>> Refund(string id, RefundRequest request, CancellationToken cancellationToken)
    {
        return Ok(ApiResponse<PaymentResponse>.Ok(await _paymentService.RefundAsync(id, request, cancellationToken), "Payment refunded"));
    }

    [HttpGet("reconciliation")]
    public async Task<ActionResult<ApiResponse<PaymentReconciliationResponse>>> Reconciliation(CancellationToken cancellationToken)
    {
        return Ok(ApiResponse<PaymentReconciliationResponse>.Ok(await _paymentService.GetReconciliationAsync(cancellationToken)));
    }
}
