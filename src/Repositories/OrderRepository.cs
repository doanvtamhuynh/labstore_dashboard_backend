using backend.src.DTOs;
using backend.src.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace backend.src.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly IMongoCollection<Order> _collection;

    public OrderRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Order>("orders");
    }

    public async Task<(IReadOnlyList<Order> Items, long Total)> ListAsync(OrderQuery query, CancellationToken cancellationToken)
    {
        var filters = new List<FilterDefinition<Order>>();
        var builder = Builders<Order>.Filter;

        if (query.Status.HasValue)
        {
            filters.Add(builder.Eq(order => order.Status, query.Status.Value));
        }

        if (!string.IsNullOrWhiteSpace(query.Customer))
        {
            var keyword = query.Customer.Trim();
            filters.Add(builder.Or(
                builder.Regex(order => order.CustomerName, new BsonRegularExpression(keyword, "i")),
                builder.Regex(order => order.CustomerEmail, new BsonRegularExpression(keyword, "i")),
                builder.Regex(order => order.Code, new BsonRegularExpression(keyword, "i"))));
        }

        if (query.FromDate.HasValue)
        {
            filters.Add(builder.Gte(order => order.CreatedAtUtc, query.FromDate.Value));
        }

        if (query.ToDate.HasValue)
        {
            filters.Add(builder.Lte(order => order.CreatedAtUtc, query.ToDate.Value));
        }

        var filter = filters.Count == 0 ? builder.Empty : builder.And(filters);
        var page = Math.Max(query.Page, 1);
        var limit = Math.Clamp(query.Limit, 1, 100);

        var totalTask = _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var itemsTask = _collection.Find(filter)
            .SortByDescending(order => order.CreatedAtUtc)
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToListAsync(cancellationToken);

        await Task.WhenAll(totalTask, itemsTask);
        return (itemsTask.Result, totalTask.Result);
    }

    public async Task<Order?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return await _collection.Find(order => order.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public Task UpdateAsync(Order order, CancellationToken cancellationToken)
    {
        order.UpdatedAtUtc = DateTime.UtcNow;
        return _collection.ReplaceOneAsync(item => item.Id == order.Id, order, cancellationToken: cancellationToken);
    }
}
