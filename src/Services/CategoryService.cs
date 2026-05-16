using backend.src.DTOs;
using backend.src.Models;
using backend.src.Repositories;

namespace backend.src.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categories;

    public CategoryService(ICategoryRepository categories)
    {
        _categories = categories;
    }

    public async Task<IReadOnlyList<CategoryResponse>> ListTreeAsync(CancellationToken cancellationToken)
    {
        var categories = await _categories.ListAsync(cancellationToken);
        return BuildTree(categories, null);
    }

    public async Task<CategoryResponse> CreateAsync(CategoryRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        var category = FromRequest(request);
        await _categories.CreateAsync(category, cancellationToken);
        return ToResponse(category, []);
    }

    public async Task<CategoryResponse> UpdateAsync(string id, CategoryRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        var category = await GetCategoryAsync(id, cancellationToken);
        category.Name = request.Name.Trim();
        category.Slug = string.IsNullOrWhiteSpace(request.Slug) ? Slugify(request.Name) : request.Slug.Trim();
        category.ParentId = request.ParentId;
        category.ImageUrl = request.ImageUrl;
        category.SortOrder = request.SortOrder;
        category.IsActive = request.IsActive;
        await _categories.UpdateAsync(category, cancellationToken);
        return ToResponse(category, []);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var all = await _categories.ListAsync(cancellationToken);
        if (all.Any(category => category.ParentId == id))
        {
            throw new InvalidOperationException("Cannot delete a category that has child categories");
        }

        await _categories.DeleteAsync(id, cancellationToken);
    }

    public async Task ReorderAsync(CategoryReorderRequest request, CancellationToken cancellationToken)
    {
        var all = await _categories.ListAsync(cancellationToken);
        var map = all.ToDictionary(category => category.Id!, category => category);
        var changed = new List<Category>();
        foreach (var item in request.Items)
        {
            if (!map.TryGetValue(item.Id, out var category))
            {
                continue;
            }

            category.SortOrder = item.SortOrder;
            category.ParentId = item.ParentId;
            category.UpdatedAtUtc = DateTime.UtcNow;
            changed.Add(category);
        }

        await _categories.ReorderAsync(changed, cancellationToken);
    }

    private async Task<Category> GetCategoryAsync(string id, CancellationToken cancellationToken)
    {
        return await _categories.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Category not found");
    }

    private static Category FromRequest(CategoryRequest request)
    {
        return new Category
        {
            Name = request.Name.Trim(),
            Slug = string.IsNullOrWhiteSpace(request.Slug) ? Slugify(request.Name) : request.Slug.Trim(),
            ParentId = request.ParentId,
            ImageUrl = request.ImageUrl,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };
    }

    private static IReadOnlyList<CategoryResponse> BuildTree(IReadOnlyList<Category> categories, string? parentId)
    {
        return categories
            .Where(category => category.ParentId == parentId)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .Select(category => ToResponse(category, BuildTree(categories, category.Id)))
            .ToList();
    }

    private static CategoryResponse ToResponse(Category category, IReadOnlyList<CategoryResponse> children)
    {
        return new CategoryResponse(
            category.Id!,
            category.Name,
            category.Slug,
            category.ParentId,
            category.ImageUrl,
            category.SortOrder,
            category.IsActive,
            children);
    }

    private static void Validate(CategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidOperationException("Category name is required");
        }
    }

    private static string Slugify(string value)
    {
        var chars = value.Trim().ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray();
        return string.Join('-', new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
    }
}
