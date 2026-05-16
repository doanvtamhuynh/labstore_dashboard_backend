using backend.src.DTOs;
using backend.src.Models;
using MongoDB.Driver;

namespace backend.src.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _collection;

    public ProductRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Product>("products");
    }

    public async Task<(IReadOnlyList<Product> Items, long Total)> ListAsync(ProductQuery query, CancellationToken cancellationToken)
    {
        var filters = new List<FilterDefinition<Product>>();
        var builder = Builders<Product>.Filter;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim();
            filters.Add(builder.Or(
                builder.Regex(product => product.Name, new MongoDB.Bson.BsonRegularExpression(keyword, "i")),
                builder.Regex(product => product.Sku, new MongoDB.Bson.BsonRegularExpression(keyword, "i"))));
        }

        if (!string.IsNullOrWhiteSpace(query.CategoryId))
        {
            filters.Add(builder.Eq(product => product.CategoryId, query.CategoryId));
        }

        if (query.Status.HasValue)
        {
            filters.Add(builder.Eq(product => product.Status, query.Status.Value));
        }

        var filter = filters.Count == 0 ? builder.Empty : builder.And(filters);
        var sort = BuildSort(query.SortBy, query.SortDirection);
        var page = Math.Max(query.Page, 1);
        var limit = Math.Clamp(query.Limit, 1, 100);

        var totalTask = _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var itemsTask = _collection.Find(filter)
            .Sort(sort)
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToListAsync(cancellationToken);

        await Task.WhenAll(totalTask, itemsTask);
        return (itemsTask.Result, totalTask.Result);
    }

    public async Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return await _collection.Find(product => product.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public Task CreateAsync(Product product, CancellationToken cancellationToken)
    {
        return _collection.InsertOneAsync(product, cancellationToken: cancellationToken);
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        product.UpdatedAtUtc = DateTime.UtcNow;
        return _collection.ReplaceOneAsync(item => item.Id == product.Id, product, cancellationToken: cancellationToken);
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken)
    {
        return _collection.DeleteOneAsync(product => product.Id == id, cancellationToken);
    }

    private static SortDefinition<Product> BuildSort(string? sortBy, string? sortDirection)
    {
        var sortBuilder = Builders<Product>.Sort;
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return sortBy?.ToLowerInvariant() switch
        {
            "name" => descending ? sortBuilder.Descending(product => product.Name) : sortBuilder.Ascending(product => product.Name),
            "price" => descending ? sortBuilder.Descending(product => product.Price) : sortBuilder.Ascending(product => product.Price),
            "stock" => descending ? sortBuilder.Descending(product => product.Stock) : sortBuilder.Ascending(product => product.Stock),
            _ => sortBuilder.Descending(product => product.CreatedAtUtc)
        };
    }
}
