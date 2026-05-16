using backend.src.DTOs;
using backend.src.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace backend.src.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly IMongoCollection<Customer> _collection;

    public CustomerRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Customer>("customers");
    }

    public async Task<(IReadOnlyList<Customer> Items, long Total)> ListAsync(CustomerQuery query, CancellationToken cancellationToken)
    {
        var filters = new List<FilterDefinition<Customer>>();
        var builder = Builders<Customer>.Filter;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim();
            filters.Add(builder.Or(
                builder.Regex(customer => customer.FullName, new BsonRegularExpression(keyword, "i")),
                builder.Regex(customer => customer.Email, new BsonRegularExpression(keyword, "i")),
                builder.Regex(customer => customer.Phone, new BsonRegularExpression(keyword, "i"))));
        }

        if (!string.IsNullOrWhiteSpace(query.Segment))
        {
            filters.Add(builder.Eq(customer => customer.Segment, query.Segment));
        }

        if (query.Status.HasValue)
        {
            filters.Add(builder.Eq(customer => customer.Status, query.Status.Value));
        }

        var filter = filters.Count == 0 ? builder.Empty : builder.And(filters);
        var page = Math.Max(query.Page, 1);
        var limit = Math.Clamp(query.Limit, 1, 100);

        var totalTask = _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var itemsTask = _collection.Find(filter)
            .SortByDescending(customer => customer.CreatedAtUtc)
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToListAsync(cancellationToken);

        await Task.WhenAll(totalTask, itemsTask);
        return (itemsTask.Result, totalTask.Result);
    }

    public async Task<Customer?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return await _collection.Find(customer => customer.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public Task UpdateAsync(Customer customer, CancellationToken cancellationToken)
    {
        customer.UpdatedAtUtc = DateTime.UtcNow;
        return _collection.ReplaceOneAsync(item => item.Id == customer.Id, customer, cancellationToken: cancellationToken);
    }
}
