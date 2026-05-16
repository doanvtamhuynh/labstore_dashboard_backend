using backend.src.DTOs;

namespace backend.src.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponse>> ListTreeAsync(CancellationToken cancellationToken);
    Task<CategoryResponse> CreateAsync(CategoryRequest request, CancellationToken cancellationToken);
    Task<CategoryResponse> UpdateAsync(string id, CategoryRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(string id, CancellationToken cancellationToken);
    Task ReorderAsync(CategoryReorderRequest request, CancellationToken cancellationToken);
}
