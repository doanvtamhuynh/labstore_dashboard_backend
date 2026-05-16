using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.src.Models;

public sealed class StoreSettings
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string StoreName { get; set; } = "Labstore";
    public string? LogoUrl { get; set; }
    public string Address { get; set; } = string.Empty;
    public string TimeZone { get; set; } = "Asia/Saigon";
}

public sealed class GeneralSettings
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public decimal TaxRate { get; set; }
    public string Currency { get; set; } = "VND";
    public string Language { get; set; } = "vi";
}
