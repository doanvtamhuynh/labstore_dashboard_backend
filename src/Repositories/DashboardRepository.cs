using backend.src.DTOs;
using MongoDB.Bson;
using MongoDB.Driver;

namespace backend.src.Repositories;

public sealed class DashboardRepository : IDashboardRepository
{
    private readonly IMongoCollection<BsonDocument> _orders;
    private readonly IMongoCollection<BsonDocument> _products;
    private readonly IMongoCollection<BsonDocument> _customers;

    public DashboardRepository(IMongoDatabase database)
    {
        _orders = database.GetCollection<BsonDocument>("orders");
        _products = database.GetCollection<BsonDocument>("products");
        _customers = database.GetCollection<BsonDocument>("customers");
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var revenue = await SumDecimalAsync(_orders, "totalAmount", cancellationToken);
        var orders = await _orders.CountDocumentsAsync(Builders<BsonDocument>.Filter.Empty, cancellationToken: cancellationToken);
        var since = DateTime.UtcNow.AddDays(-30);
        var newCustomers = await _customers.CountDocumentsAsync(
            Builders<BsonDocument>.Filter.Gte("createdAtUtc", since),
            cancellationToken: cancellationToken);
        var lowStock = await _products.CountDocumentsAsync(
            Builders<BsonDocument>.Filter.Lte("stock", 10),
            cancellationToken: cancellationToken);

        return new DashboardSummaryResponse(revenue, orders, newCustomers, lowStock);
    }

    public async Task<IReadOnlyList<RevenueChartPoint>> GetRevenueChartAsync(string period, CancellationToken cancellationToken)
    {
        var dateFormat = period.ToLowerInvariant() switch
        {
            "day" => "%Y-%m-%d",
            "week" => "%Y-W%V",
            "year" => "%Y",
            _ => "%Y-%m"
        };

        var pipeline = new[]
        {
            new BsonDocument("$group", new BsonDocument
            {
                ["_id"] = new BsonDocument("$dateToString", new BsonDocument
                {
                    ["format"] = dateFormat,
                    ["date"] = "$createdAtUtc"
                }),
                ["revenue"] = new BsonDocument("$sum", "$totalAmount"),
                ["orders"] = new BsonDocument("$sum", 1)
            }),
            new BsonDocument("$sort", new BsonDocument("_id", 1))
        };

        var documents = await _orders.Aggregate<BsonDocument>(pipeline).ToListAsync(cancellationToken);
        return documents.Select(doc => new RevenueChartPoint(
            doc.GetValue("_id", "unknown").ToString() ?? "unknown",
            ToDecimal(doc.GetValue("revenue", 0)),
            doc.GetValue("orders", 0).ToInt64())).ToList();
    }

    public async Task<IReadOnlyList<TopProductResponse>> GetTopProductsAsync(int limit, CancellationToken cancellationToken)
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
            new BsonDocument("$limit", Math.Clamp(limit, 1, 50))
        };

        var documents = await _orders.Aggregate<BsonDocument>(pipeline).ToListAsync(cancellationToken);
        return documents.Select(doc => new TopProductResponse(
            doc.GetValue("_id", string.Empty).ToString() ?? string.Empty,
            doc.GetValue("name", "Unknown product").ToString() ?? "Unknown product",
            doc.GetValue("quantitySold", 0).ToInt64(),
            ToDecimal(doc.GetValue("revenue", 0)))).ToList();
    }

    public async Task<IReadOnlyList<GeoOrderResponse>> GetGeoOrdersAsync(CancellationToken cancellationToken)
    {
        var pipeline = new[]
        {
            new BsonDocument("$group", new BsonDocument
            {
                ["_id"] = "$shippingAddress.province",
                ["orders"] = new BsonDocument("$sum", 1)
            }),
            new BsonDocument("$sort", new BsonDocument("orders", -1))
        };

        var documents = await _orders.Aggregate<BsonDocument>(pipeline).ToListAsync(cancellationToken);
        return documents.Select(doc => new GeoOrderResponse(
            doc.GetValue("_id", "Unknown").ToString() ?? "Unknown",
            doc.GetValue("orders", 0).ToInt64())).ToList();
    }

    public async Task<DashboardKpiResponse> GetKpiAsync(CancellationToken cancellationToken)
    {
        var orderCount = await _orders.CountDocumentsAsync(Builders<BsonDocument>.Filter.Empty, cancellationToken: cancellationToken);
        var customerCount = await _customers.CountDocumentsAsync(Builders<BsonDocument>.Filter.Empty, cancellationToken: cancellationToken);
        var revenue = await SumDecimalAsync(_orders, "totalAmount", cancellationToken);

        var conversionRate = customerCount == 0 ? 0 : Math.Round((decimal)orderCount / customerCount * 100, 2);
        var averageOrderValue = orderCount == 0 ? 0 : Math.Round(revenue / orderCount, 2);
        return new DashboardKpiResponse(conversionRate, averageOrderValue);
    }

    private static async Task<decimal> SumDecimalAsync(IMongoCollection<BsonDocument> collection, string field, CancellationToken cancellationToken)
    {
        var pipeline = new[]
        {
            new BsonDocument("$group", new BsonDocument
            {
                ["_id"] = BsonNull.Value,
                ["total"] = new BsonDocument("$sum", $"${field}")
            })
        };

        var result = await collection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync(cancellationToken);
        return result is null ? 0 : ToDecimal(result.GetValue("total", 0));
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
