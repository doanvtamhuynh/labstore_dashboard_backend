using backend.src.Models;

namespace backend.src.DTOs;

public sealed record ProductQuery(
    string? Search,
    string? CategoryId,
    ProductStatus? Status,
    string? SortBy,
    string? SortDirection,
    int Page = 1,
    int Limit = 20);

public sealed record ProductVariantRequest(
    string? Id,
    string Name,
    string Sku,
    decimal Price,
    int Stock,
    Dictionary<string, string>? Attributes);

public sealed record ProductImageRequest(string Url, string? Alt, int SortOrder = 0);

public sealed record SeoMetaRequest(string? Title, string? Description, string? Slug);

public sealed record ProductRequest(
    string Name,
    string? Slug,
    string? Description,
    string? CategoryId,
    string Sku,
    decimal Price,
    decimal? SalePrice,
    int Stock,
    ProductStatus Status,
    IReadOnlyList<ProductVariantRequest>? Variants,
    IReadOnlyList<ProductImageRequest>? Images,
    SeoMetaRequest? Seo);

public sealed record ProductStatusRequest(ProductStatus Status);

public sealed record ProductVariantResponse(
    string Id,
    string Name,
    string Sku,
    decimal Price,
    int Stock,
    Dictionary<string, string> Attributes);

public sealed record ProductImageResponse(string Id, string Url, string? Alt, int SortOrder);

public sealed record SeoMetaResponse(string? Title, string? Description, string? Slug);

public sealed record ProductResponse(
    string Id,
    string Name,
    string Slug,
    string? Description,
    string? CategoryId,
    string Sku,
    decimal Price,
    decimal? SalePrice,
    int Stock,
    ProductStatus Status,
    IReadOnlyList<ProductVariantResponse> Variants,
    IReadOnlyList<ProductImageResponse> Images,
    SeoMetaResponse Seo,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record ProductImportResult(int Created, int Skipped);
