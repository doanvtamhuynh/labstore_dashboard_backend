using backend.src.DTOs;
using backend.src.Helpers;
using backend.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[Authorize]
[ApiController]
[Route("api/support")]
public sealed class SupportController : ControllerBase
{
    private readonly ISupportService _supportService;

    public SupportController(ISupportService supportService)
    {
        _supportService = supportService;
    }

    [HttpGet("tickets")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TicketResponse>>>> ListTickets(CancellationToken cancellationToken) => Ok(ApiResponse<IReadOnlyList<TicketResponse>>.Ok(await _supportService.ListTicketsAsync(cancellationToken)));

    [HttpPost("tickets")]
    public async Task<ActionResult<ApiResponse<TicketResponse>>> CreateTicket(TicketRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<TicketResponse>.Ok(await _supportService.CreateTicketAsync(request, cancellationToken), "Ticket created"));

    [HttpPut("tickets/{id}")]
    public async Task<ActionResult<ApiResponse<TicketResponse>>> UpdateTicket(string id, TicketRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<TicketResponse>.Ok(await _supportService.UpdateTicketAsync(id, request, cancellationToken), "Ticket updated"));

    [HttpDelete("tickets/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteTicket(string id, CancellationToken cancellationToken)
    {
        await _supportService.DeleteTicketAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Ticket deleted"));
    }

    [HttpPatch("tickets/{id}/assign")]
    public async Task<ActionResult<ApiResponse<TicketResponse>>> AssignTicket(string id, TicketAssignRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<TicketResponse>.Ok(await _supportService.AssignTicketAsync(id, request, cancellationToken), "Ticket assigned"));

    [HttpPatch("tickets/{id}/status")]
    public async Task<ActionResult<ApiResponse<TicketResponse>>> UpdateTicketStatus(string id, TicketStatusRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<TicketResponse>.Ok(await _supportService.UpdateTicketStatusAsync(id, request, cancellationToken), "Ticket status updated"));

    [HttpGet("faq")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FaqResponse>>>> ListFaq(CancellationToken cancellationToken) => Ok(ApiResponse<IReadOnlyList<FaqResponse>>.Ok(await _supportService.ListFaqAsync(cancellationToken)));

    [HttpPost("faq")]
    public async Task<ActionResult<ApiResponse<FaqResponse>>> CreateFaq(FaqRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<FaqResponse>.Ok(await _supportService.CreateFaqAsync(request, cancellationToken), "FAQ created"));

    [HttpPut("faq/{id}")]
    public async Task<ActionResult<ApiResponse<FaqResponse>>> UpdateFaq(string id, FaqRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<FaqResponse>.Ok(await _supportService.UpdateFaqAsync(id, request, cancellationToken), "FAQ updated"));

    [HttpDelete("faq/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteFaq(string id, CancellationToken cancellationToken)
    {
        await _supportService.DeleteFaqAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "FAQ deleted"));
    }
}
