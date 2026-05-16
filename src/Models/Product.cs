using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.src.Models;

public enum ProductStatus
{
    Visible,
    Hidden,
    OutOfStock
}

public sealed class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CategoryId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public int Stock { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Visible;
    public List<ProductVariant> Variants { get; set; } = [];
    public List<ProductImage> Images { get; set; } = [];
    public SeoMeta Seo { get; set; } = new();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class ProductVariant
{
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public Dictionary<string, string> Attributes { get; set; } = [];
}

public sealed class ProductImage
{
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string Url { get; set; } = string.Empty;
    public string? Alt { get; set; }
    public int SortOrder { get; set; }
}

public sealed class SeoMeta
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Slug { get; set; }
}
