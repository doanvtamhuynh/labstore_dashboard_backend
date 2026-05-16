using backend.src.DTOs;
using backend.src.Helpers;
using backend.src.Models;
using backend.src.Repositories;

namespace backend.src.Services;

public sealed class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviews;

    public ReviewService(IReviewRepository reviews)
    {
        _reviews = reviews;
    }

    public async Task<(IReadOnlyList<ReviewResponse> Items, PaginationMetadata Pagination)> ListAsync(ReviewQuery query, CancellationToken cancellationToken)
    {
        var (items, total) = await _reviews.ListAsync(query, cancellationToken);
        var page = Math.Max(query.Page, 1);
        var limit = Math.Clamp(query.Limit, 1, 100);
        return (items.Select(ToResponse).ToList(), new PaginationMetadata(page, limit, total));
    }

    public async Task<ReviewResponse> UpdateStatusAsync(string id, ReviewStatusRequest request, CancellationToken cancellationToken)
    {
        var review = await GetReviewAsync(id, cancellationToken);
        review.Status = request.Status;
        await _reviews.UpdateAsync(review, cancellationToken);
        return ToResponse(review);
    }

    public async Task<ReviewResponse> ReplyAsync(string id, ReviewReplyRequest request, string? repliedBy, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reply))
        {
            throw new InvalidOperationException("Reply content is required");
        }

        var review = await GetReviewAsync(id, cancellationToken);
        review.Reply = request.Reply.Trim();
        review.RepliedBy = repliedBy;
        review.RepliedAtUtc = DateTime.UtcNow;
        await _reviews.UpdateAsync(review, cancellationToken);
        return ToResponse(review);
    }

    public async Task<IReadOnlyList<ReviewResponse>> ListFlaggedAsync(CancellationToken cancellationToken)
    {
        return (await _reviews.ListFlaggedAsync(cancellationToken)).Select(ToResponse).ToList();
    }

    private async Task<Review> GetReviewAsync(string id, CancellationToken cancellationToken)
    {
        return await _reviews.GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Review not found");
    }

    private static ReviewResponse ToResponse(Review review)
    {
        return new ReviewResponse(review.Id!, review.ProductId, review.CustomerId, review.CustomerName, review.Rating, review.Comment, review.Status, review.IsFlagged, review.Reply, review.RepliedBy, review.RepliedAtUtc, review.CreatedAtUtc);
    }
}
