using backend.src.DTOs;
using MongoDB.Bson;
using MongoDB.Driver;

namespace backend.src.Repositories;

public sealed class ReportRepository : IReportRepository
{
    private readonly IMongoCollection<BsonDocument> _orders;
    private readonly IMongoCollection<BsonDocument> _products;
    private readonly IMongoCollection<BsonDocument> _customers;
    private readonly IMongoCollection<BsonDocument> _affiliates;

    public ReportRepository(IMongoDatabase database)
    {
        _orders = database.GetCollection<BsonDocument>("orders");
        _products = database.GetCollection<BsonDocument>("products");
        _customers = database.GetCollection<BsonDocument>("customers");
        _affiliates = database.GetCollection<BsonDocument>("promotion_affiliates");
    }

    public async Task<RevenueReportResponse> GetRevenueAsync(DateRangeQuery query, CancellationToken cancellationToken)
    {
        var filter = BuildDateFilter(query);
        var orders = await _orders.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var result = await _orders.Aggregate()
            .Match(filter)
            .Group(new BsonDocument { ["_id"] = BsonNull.Value, ["total"] = new BsonDocument("$sum", "$totalAmount") })
            .FirstOrDefaultAsync(cancellationToken);
        return new RevenueReportResponse(result is null ? 0 : ToDecimal(result.GetValue("total", 0)), orders, query.FromDate, query.ToDate);
    }

    public async Task<IReadOnlyList<TopProductResponse>> GetProductsAsync(CancellationToken cancellationToken)
    {
        var pipeline = new[]
        {
            new BsonDocument("$unwind", "$items"),
            new BsonDocument("$group", new BsonDocument
            {
                ["_id"] = "$items.productId",
                ["name"] = new BsonDocument("$first", "$items.productName"),
                ["quantitySold"] = new BsonDocument("$sum", "$items.quantity"),
                ["revenue"] = new BsonDocument("$sum", new BsonDocument("$multiply", new BsonArray { "$items.quantity", "$items.price" }))
            }),
            new BsonDocument("$sort", new BsonDocument("quantitySold", -1)),
            new BsonDocument("$limit", 20)
        };
        var docs = await _orders.Aggregate<BsonDocument>(pipeline).ToListAsync(cancellationToken);
        return docs.Select(doc => new TopProductResponse(doc.GetValue("_id", string.Empty).ToString() ?? string.Empty, doc.GetValue("name", "Unknown").ToString() ?? "Unknown", doc.GetValue("quantitySold", 0).ToInt64(), ToDecimal(doc.GetValue("revenue", 0)))).ToList();
    }

    public async Task<InventoryReportResponse> GetInventoryAsync(CancellationToken cancellationToken)
    {
        var low = await _products.CountDocumentsAsync(Builders<BsonDocument>.Filter.Lte("stock", 10), cancellationToken: cancellationToken);
        var outOfStock = await _products.CountDocumentsAsync(Builders<BsonDocument>.Filter.Lte("stock", 0), cancellationToken: cancellationToken);
        return new InventoryReportResponse(low, outOfStock);
    }

    public async Task<CustomerBehaviorReportResponse> GetCustomersAsync(CancellationToken cancellationToken)
    {
        var customers = await _customers.CountDocumentsAsync(Builders<BsonDocument>.Filter.Empty, cancellationToken: cancellationToken);
        var orders = await _orders.CountDocumentsAsync(Builders<BsonDocument>.Filter.Empty, cancellationToken: cancellationToken);
        return new CustomerBehaviorReportResponse(customers, orders, customers == 0 ? 0 : Math.Round((decimal)orders / customers, 2));
    }

    public async Task<AffiliateReportResponse> GetAffiliateAsync(CancellationToken cancellationToken)
    {
        var partners = await _affiliates.CountDocumentsAsync(Builders<BsonDocument>.Filter.Empty, cancellationToken: cancellationToken);
        return new AffiliateReportResponse(partners, 0);
    }

    private static FilterDefinition<BsonDocument> BuildDateFilter(DateRangeQuery query)
    {
        var builder = Builders<BsonDocument>.Filter;
        var filters = new List<FilterDefinition<BsonDocument>>();
        if (query.FromDate.HasValue) filters.Add(builder.Gte("createdAtUtc", query.FromDate.Value));
        if (query.ToDate.HasValue) filters.Add(builder.Lte("createdAtUtc", query.ToDate.Value));
        return filters.Count == 0 ? builder.Empty : builder.And(filters);
    }

    private static decimal ToDecimal(BsonValue value)
    {
        return value switch
        {
            { IsDecimal128: true } => (decimal)value.AsDecimal128,
            { IsDouble: true } => (decimal)value.AsDouble,
            { IsInt32: true } => value.AsInt32,
            { IsInt64: true } => value.AsInt64,
            _ => 0
        };
    }
}
