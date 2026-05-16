using backend.src.DTOs;
using backend.src.Helpers;
using backend.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[Authorize]
[ApiController]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Get category tree with nested child categories.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CategoryResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CategoryResponse>>>> List(CancellationToken cancellationToken)
    {
        var result = await _categoryService.ListTreeAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CategoryResponse>>.Ok(result));
    }

    /// <summary>
    /// Create a category.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> Create(CategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(List), ApiResponse<CategoryResponse>.Ok(result, "Category created"));
    }

    /// <summary>
    /// Update a category.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> Update(string id, CategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<CategoryResponse>.Ok(result, "Category updated"));
    }

    /// <summary>
    /// Delete a category that has no children.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(string id, CancellationToken cancellationToken)
    {
        await _categoryService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Category deleted"));
    }

    /// <summary>
    /// Update category order and parent relationships.
    /// </summary>
    [HttpPatch("reorder")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Reorder(CategoryReorderRequest request, CancellationToken cancellationToken)
    {
        await _categoryService.ReorderAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Categories reordered"));
    }
}
