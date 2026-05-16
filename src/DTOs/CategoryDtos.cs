namespace backend.src.DTOs;

public sealed record CategoryRequest(string Name, string? Slug, string? ParentId, string? ImageUrl, int SortOrder, bool IsActive = true);

public sealed record CategoryReorderItem(string Id, int SortOrder, string? ParentId);

public sealed record CategoryReorderRequest(IReadOnlyList<CategoryReorderItem> Items);

public sealed record CategoryResponse(
    string Id,
    string Name,
    string Slug,
    string? ParentId,
    string? ImageUrl,
    int SortOrder,
    bool IsActive,
    IReadOnlyList<CategoryResponse> Children);
