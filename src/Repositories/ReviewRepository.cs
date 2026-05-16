using backend.src.DTOs;
using backend.src.Models;
using MongoDB.Driver;

namespace backend.src.Repositories;

public sealed class ReviewRepository : IReviewRepository
{
    private readonly IMongoCollection<Review> _collection;

    public ReviewRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Review>("reviews");
    }

    public async Task<(IReadOnlyList<Review> Items, long Total)> ListAsync(ReviewQuery query, CancellationToken cancellationToken)
    {
        var filters = new List<FilterDefinition<Review>>();
        var builder = Builders<Review>.Filter;
        if (query.Rating.HasValue)
        {
            filters.Add(builder.Eq(review => review.Rating, query.Rating.Value));
        }

        if (!string.IsNullOrWhiteSpace(query.ProductId))
        {
            filters.Add(builder.Eq(review => review.ProductId, query.ProductId));
        }

        if (query.Status.HasValue)
        {
            filters.Add(builder.Eq(review => review.Status, query.Status.Value));
        }

        var filter = filters.Count == 0 ? builder.Empty : builder.And(filters);
        var page = Math.Max(query.Page, 1);
        var limit = Math.Clamp(query.Limit, 1, 100);
        var totalTask = _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var itemsTask = _collection.Find(filter).SortByDescending(review => review.CreatedAtUtc).Skip((page - 1) * limit).Limit(limit).ToListAsync(cancellationToken);
        await Task.WhenAll(totalTask, itemsTask);
        return (itemsTask.Result, totalTask.Result);
    }

    public async Task<IReadOnlyList<Review>> ListFlaggedAsync(CancellationToken cancellationToken)
    {
        return await _collection.Find(review => review.IsFlagged).SortByDescending(review => review.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public async Task<Review?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return await _collection.Find(review => review.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public Task UpdateAsync(Review review, CancellationToken cancellationToken)
    {
        review.UpdatedAtUtc = DateTime.UtcNow;
        return _collection.ReplaceOneAsync(item => item.Id == review.Id, review, cancellationToken: cancellationToken);
    }
}
