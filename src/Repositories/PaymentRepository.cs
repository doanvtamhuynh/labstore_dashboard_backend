using backend.src.DTOs;
using backend.src.Models;
using MongoDB.Driver;

namespace backend.src.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly IMongoCollection<Payment> _collection;

    public PaymentRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Payment>("payments");
    }

    public async Task<(IReadOnlyList<Payment> Items, long Total)> ListAsync(PaymentQuery query, CancellationToken cancellationToken)
    {
        var filters = new List<FilterDefinition<Payment>>();
        var builder = Builders<Payment>.Filter;
        if (!string.IsNullOrWhiteSpace(query.Method))
        {
            filters.Add(builder.Eq(payment => payment.Method, query.Method));
        }

        if (query.Status.HasValue)
        {
            filters.Add(builder.Eq(payment => payment.Status, query.Status.Value));
        }

        if (query.FromDate.HasValue)
        {
            filters.Add(builder.Gte(payment => payment.CreatedAtUtc, query.FromDate.Value));
        }

        if (query.ToDate.HasValue)
        {
            filters.Add(builder.Lte(payment => payment.CreatedAtUtc, query.ToDate.Value));
        }

        var filter = filters.Count == 0 ? builder.Empty : builder.And(filters);
        var page = Math.Max(query.Page, 1);
        var limit = Math.Clamp(query.Limit, 1, 100);
        var totalTask = _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var itemsTask = _collection.Find(filter).SortByDescending(payment => payment.CreatedAtUtc).Skip((page - 1) * limit).Limit(limit).ToListAsync(cancellationToken);
        await Task.WhenAll(totalTask, itemsTask);
        return (itemsTask.Result, totalTask.Result);
    }

    public async Task<Payment?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return await _collection.Find(payment => payment.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public Task UpdateAsync(Payment payment, CancellationToken cancellationToken)
    {
        payment.UpdatedAtUtc = DateTime.UtcNow;
        return _collection.ReplaceOneAsync(item => item.Id == payment.Id, payment, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> ListSucceededAsync(CancellationToken cancellationToken)
    {
        return await _collection.Find(payment => payment.Status == PaymentTransactionStatus.Succeeded || payment.Status == PaymentTransactionStatus.Refunded).ToListAsync(cancellationToken);
    }
}
