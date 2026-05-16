using System.Text;
using backend.src.DTOs;
using backend.src.Helpers;
using backend.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[Authorize]
[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get products with search, filters, sorting, and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ProductResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProductResponse>>>> List([FromQuery] ProductQuery query, CancellationToken cancellationToken)
    {
        var (items, pagination) = await _productService.ListAsync(query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ProductResponse>>.Ok(items, pagination: pagination));
    }

    /// <summary>
    /// Get product detail by id.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> GetById(string id, CancellationToken cancellationToken)
    {
        var result = await _productService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ProductResponse>.Ok(result));
    }

    /// <summary>
    /// Create a product with variants, images, and SEO metadata.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> Create(ProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ProductResponse>.Ok(result, "Product created"));
    }

    /// <summary>
    /// Update a product.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> Update(string id, ProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<ProductResponse>.Ok(result, "Product updated"));
    }

    /// <summary>
    /// Delete a product.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(string id, CancellationToken cancellationToken)
    {
        await _productService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Product deleted"));
    }

    /// <summary>
    /// Change product visibility or inventory status.
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> ChangeStatus(string id, ProductStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.ChangeStatusAsync(id, request, cancellationToken);
        return Ok(ApiResponse<ProductResponse>.Ok(result, "Product status updated"));
    }

    /// <summary>
    /// Upload an image file or add an image URL to a product.
    /// </summary>
    [HttpPost("{id}/images")]
    [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> AddImage(string id, [FromForm] ProductImageUploadRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.AddImageAsync(id, request, cancellationToken);
        return Ok(ApiResponse<ProductResponse>.Ok(result, "Product image added"));
    }

    /// <summary>
    /// Delete a product image.
    /// </summary>
    [HttpDelete("{id}/images/{imageId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteImage(string id, string imageId, CancellationToken cancellationToken)
    {
        await _productService.DeleteImageAsync(id, imageId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Product image deleted"));
    }

    /// <summary>
    /// Import products from a CSV file with columns name, sku, price, stock, categoryId.
    /// </summary>
    [HttpPost("import")]
    [ProducesResponseType(typeof(ApiResponse<ProductImportResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProductImportResult>>> Import(IFormFile file, CancellationToken cancellationToken)
    {
        var result = await _productService.ImportCsvAsync(file, cancellationToken);
        return Ok(ApiResponse<ProductImportResult>.Ok(result, "Product import completed"));
    }

    /// <summary>
    /// Export products to CSV.
    /// </summary>
    [HttpGet("export")]
    [Produces("text/csv")]
    public async Task<FileResult> Export([FromQuery] ProductQuery query, CancellationToken cancellationToken)
    {
        var csv = await _productService.ExportCsvAsync(query, cancellationToken);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", "products.csv");
    }
}
