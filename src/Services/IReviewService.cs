using backend.src.DTOs;
using backend.src.Helpers;

namespace backend.src.Services;

public interface IReviewService
{
    Task<(IReadOnlyList<ReviewResponse> Items, PaginationMetadata Pagination)> ListAsync(ReviewQuery query, CancellationToken cancellationToken);
    Task<ReviewResponse> UpdateStatusAsync(string id, ReviewStatusRequest request, CancellationToken cancellationToken);
    Task<ReviewResponse> ReplyAsync(string id, ReviewReplyRequest request, string? repliedBy, CancellationToken cancellationToken);
    Task<IReadOnlyList<ReviewResponse>> ListFlaggedAsync(CancellationToken cancellationToken);
}
