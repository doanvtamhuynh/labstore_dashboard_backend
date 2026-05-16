using System.Globalization;
using System.Text;
using backend.src.DTOs;
using backend.src.Helpers;
using backend.src.Models;
using backend.src.Repositories;

namespace backend.src.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _products;
    private readonly IFileStorageService _fileStorage;

    public ProductService(IProductRepository products, IFileStorageService fileStorage)
    {
        _products = products;
        _fileStorage = fileStorage;
    }

    public async Task<(IReadOnlyList<ProductResponse> Items, PaginationMetadata Pagination)> ListAsync(ProductQuery query, CancellationToken cancellationToken)
    {
        var (items, total) = await _products.ListAsync(query, cancellationToken);
        var page = Math.Max(query.Page, 1);
        var limit = Math.Clamp(query.Limit, 1, 100);
        return (items.Select(ToResponse).ToList(), new PaginationMetadata(page, limit, total));
    }

    public async Task<ProductResponse> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return ToResponse(await GetProductAsync(id, cancellationToken));
    }

    public async Task<ProductResponse> CreateAsync(ProductRequest request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);
        var product = FromRequest(request);
        await _products.CreateAsync(product, cancellationToken);
        return ToResponse(product);
    }

    public async Task<ProductResponse> UpdateAsync(string id, ProductRequest request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);
        var existing = await GetProductAsync(id, cancellationToken);
        var updated = FromRequest(request);
        updated.Id = existing.Id;
        updated.CreatedAtUtc = existing.CreatedAtUtc;
        updated.UpdatedAtUtc = DateTime.UtcNow;
        await _products.UpdateAsync(updated, cancellationToken);
        return ToResponse(updated);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken)
    {
        _ = await GetProductAsync(id, cancellationToken);
        await _products.DeleteAsync(id, cancellationToken);
    }

    public async Task<ProductResponse> ChangeStatusAsync(string id, ProductStatusRequest request, CancellationToken cancellationToken)
    {
        var product = await GetProductAsync(id, cancellationToken);
        product.Status = request.Status;
        await _products.UpdateAsync(product, cancellationToken);
        return ToResponse(product);
    }

    public async Task<ProductResponse> AddImageAsync(string id, ProductImageUploadRequest request, CancellationToken cancellationToken)
    {
        var imageUrl = request.File is not null
            ? await _fileStorage.UploadImageAsync(request.File, "labstore/products", cancellationToken)
            : request.Url?.Trim();

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            throw new InvalidOperationException("Image file or URL is required");
        }

        var product = await GetProductAsync(id, cancellationToken);
        product.Images.Add(new ProductImage
        {
            Url = imageUrl,
            Alt = request.Alt,
            SortOrder = request.SortOrder
        });
        await _products.UpdateAsync(product, cancellationToken);
        return ToResponse(product);
    }

    public async Task DeleteImageAsync(string id, string imageId, CancellationToken cancellationToken)
    {
        var product = await GetProductAsync(id, cancellationToken);
        product.Images = product.Images.Where(image => image.Id != imageId).ToList();
        await _products.UpdateAsync(product, cancellationToken);
    }

    public async Task<ProductImportResult> ImportCsvAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            throw new InvalidOperationException("CSV file is empty");
        }

        var created = 0;
        var skipped = 0;
        using var reader = new StreamReader(file.OpenReadStream());
        var header = await reader.ReadLineAsync(cancellationToken);
        if (header is null)
        {
            return new ProductImportResult(0, 0);
        }

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                skipped++;
                continue;
            }

            var columns = line.Split(',');
            if (columns.Length < 5 ||
                !decimal.TryParse(columns[2], NumberStyles.Number, CultureInfo.InvariantCulture, out var price) ||
                !int.TryParse(columns[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var stock))
            {
                skipped++;
                continue;
            }

            await _products.CreateAsync(new Product
            {
                Name = columns[0].Trim(),
                Slug = Slugify(columns[0]),
                Sku = columns[1].Trim(),
                Price = price,
                Stock = stock,
                CategoryId = string.IsNullOrWhiteSpace(columns[4]) ? null : columns[4].Trim(),
                Status = stock > 0 ? ProductStatus.Visible : ProductStatus.OutOfStock,
                Seo = new SeoMeta { Slug = Slugify(columns[0]) }
            }, cancellationToken);
            created++;
        }

        return new ProductImportResult(created, skipped);
    }

    public async Task<string> ExportCsvAsync(ProductQuery query, CancellationToken cancellationToken)
    {
        var exportQuery = query with { Page = 1, Limit = 1000 };
        var (items, _) = await _products.ListAsync(exportQuery, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine("id,name,sku,price,salePrice,stock,status,categoryId");
        foreach (var product in items)
        {
            builder.AppendLine(string.Join(',', new[]
            {
                Escape(product.Id),
                Escape(product.Name),
                Escape(product.Sku),
                product.Price.ToString(CultureInfo.InvariantCulture),
                product.SalePrice?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                product.Stock.ToString(CultureInfo.InvariantCulture),
                product.Status.ToString(),
                Escape(product.CategoryId)
            }));
        }

        return builder.ToString();
    }

    private async Task<Product> GetProductAsync(string id, CancellationToken cancellationToken)
    {
        return await _products.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Product not found");
    }

    private static Product FromRequest(ProductRequest request)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug) ? Slugify(request.Name) : request.Slug.Trim();
        return new Product
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Sku = request.Sku.Trim(),
            Price = request.Price,
            SalePrice = request.SalePrice,
            Stock = request.Stock,
            Status = request.Status,
            Variants = request.Variants?.Select(variant => new ProductVariant
            {
                Id = string.IsNullOrWhiteSpace(variant.Id) ? MongoDB.Bson.ObjectId.GenerateNewId().ToString() : variant.Id,
                Name = variant.Name.Trim(),
                Sku = variant.Sku.Trim(),
                Price = variant.Price,
                Stock = variant.Stock,
                Attributes = variant.Attributes ?? []
            }).ToList() ?? [],
            Images = request.Images?.Select(image => new ProductImage
            {
                Url = image.Url.Trim(),
                Alt = image.Alt,
                SortOrder = image.SortOrder
            }).ToList() ?? [],
            Seo = new SeoMeta
            {
                Title = request.Seo?.Title,
                Description = request.Seo?.Description,
                Slug = request.Seo?.Slug ?? slug
            }
        };
    }

    private static ProductResponse ToResponse(Product product)
    {
        return new ProductResponse(
            product.Id!,
            product.Name,
            product.Slug,
            product.Description,
            product.CategoryId,
            product.Sku,
            product.Price,
            product.SalePrice,
            product.Stock,
            product.Status,
            product.Variants.Select(variant => new ProductVariantResponse(variant.Id, variant.Name, variant.Sku, variant.Price, variant.Stock, variant.Attributes)).ToList(),
            product.Images.OrderBy(image => image.SortOrder).Select(image => new ProductImageResponse(image.Id, image.Url, image.Alt, image.SortOrder)).ToList(),
            new SeoMetaResponse(product.Seo.Title, product.Seo.Description, product.Seo.Slug),
            product.CreatedAtUtc,
            product.UpdatedAtUtc);
    }

    private static void ValidateRequest(ProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidOperationException("Product name is required");
        }

        if (string.IsNullOrWhiteSpace(request.Sku))
        {
            throw new InvalidOperationException("Product SKU is required");
        }

        if (request.Price < 0 || request.Stock < 0)
        {
            throw new InvalidOperationException("Product price and stock must be positive");
        }
    }

    private static string Slugify(string value)
    {
        var chars = value.Trim().ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray();
        return string.Join('-', new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Contains(',') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    }
}
