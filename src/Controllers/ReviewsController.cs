using System.Security.Claims;
using backend.src.DTOs;
using backend.src.Helpers;
using backend.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[Authorize]
[ApiController]
[Route("api/reviews")]
public sealed class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ReviewResponse>>>> List([FromQuery] ReviewQuery query, CancellationToken cancellationToken)
    {
        var (items, pagination) = await _reviewService.ListAsync(query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ReviewResponse>>.Ok(items, pagination: pagination));
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> UpdateStatus(string id, ReviewStatusRequest request, CancellationToken cancellationToken)
    {
        return Ok(ApiResponse<ReviewResponse>.Ok(await _reviewService.UpdateStatusAsync(id, request, cancellationToken), "Review status updated"));
    }

    [HttpPost("{id}/reply")]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> Reply(string id, ReviewReplyRequest request, CancellationToken cancellationToken)
    {
        var repliedBy = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Ok(ApiResponse<ReviewResponse>.Ok(await _reviewService.ReplyAsync(id, request, repliedBy, cancellationToken), "Review replied"));
    }

    [HttpGet("flagged")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ReviewResponse>>>> Flagged(CancellationToken cancellationToken)
    {
        return Ok(ApiResponse<IReadOnlyList<ReviewResponse>>.Ok(await _reviewService.ListFlaggedAsync(cancellationToken)));
    }
}
