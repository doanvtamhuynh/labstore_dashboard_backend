using backend.src.DTOs;
using backend.src.Models;

namespace backend.src.Repositories;

public interface IReviewRepository
{
    Task<(IReadOnlyList<Review> Items, long Total)> ListAsync(ReviewQuery query, CancellationToken cancellationToken);
    Task<IReadOnlyList<Review>> ListFlaggedAsync(CancellationToken cancellationToken);
    Task<Review?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task UpdateAsync(Review review, CancellationToken cancellationToken);
}
