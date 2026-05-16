using backend.src.DTOs;
using backend.src.Helpers;
using backend.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[Authorize]
[ApiController]
[Route("api/shipping")]
public sealed class ShippingController : ControllerBase
{
    private readonly IShippingService _shippingService;

    public ShippingController(IShippingService shippingService)
    {
        _shippingService = shippingService;
    }

    [HttpGet("configs")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ShippingConfigResponse>>>> ListConfigs(CancellationToken cancellationToken) => Ok(ApiResponse<IReadOnlyList<ShippingConfigResponse>>.Ok(await _shippingService.ListConfigsAsync(cancellationToken)));

    [HttpPost("configs")]
    public async Task<ActionResult<ApiResponse<ShippingConfigResponse>>> CreateConfig(ShippingConfigRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<ShippingConfigResponse>.Ok(await _shippingService.CreateConfigAsync(request, cancellationToken), "Shipping config created"));

    [HttpPut("configs/{id}")]
    public async Task<ActionResult<ApiResponse<ShippingConfigResponse>>> UpdateConfig(string id, ShippingConfigRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<ShippingConfigResponse>.Ok(await _shippingService.UpdateConfigAsync(id, request, cancellationToken), "Shipping config updated"));

    [HttpDelete("configs/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteConfig(string id, CancellationToken cancellationToken)
    {
        await _shippingService.DeleteConfigAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Shipping config deleted"));
    }

    [HttpGet("providers")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ShippingProviderResponse>>>> ListProviders(CancellationToken cancellationToken) => Ok(ApiResponse<IReadOnlyList<ShippingProviderResponse>>.Ok(await _shippingService.ListProvidersAsync(cancellationToken)));

    [HttpGet("tracking/{orderId}")]
    public async Task<ActionResult<ApiResponse<ShippingTrackingResponse>>> GetTracking(string orderId, CancellationToken cancellationToken) => Ok(ApiResponse<ShippingTrackingResponse>.Ok(await _shippingService.GetTrackingAsync(orderId, cancellationToken)));

    [HttpGet("warehouses")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WarehouseResponse>>>> ListWarehouses(CancellationToken cancellationToken) => Ok(ApiResponse<IReadOnlyList<WarehouseResponse>>.Ok(await _shippingService.ListWarehousesAsync(cancellationToken)));

    [HttpPost("warehouses")]
    public async Task<ActionResult<ApiResponse<WarehouseResponse>>> CreateWarehouse(WarehouseRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<WarehouseResponse>.Ok(await _shippingService.CreateWarehouseAsync(request, cancellationToken), "Warehouse created"));

    [HttpPut("warehouses/{id}")]
    public async Task<ActionResult<ApiResponse<WarehouseResponse>>> UpdateWarehouse(string id, WarehouseRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<WarehouseResponse>.Ok(await _shippingService.UpdateWarehouseAsync(id, request, cancellationToken), "Warehouse updated"));

    [HttpDelete("warehouses/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteWarehouse(string id, CancellationToken cancellationToken)
    {
        await _shippingService.DeleteWarehouseAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Warehouse deleted"));
    }

    [HttpGet("returns")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ShippingReturnResponse>>>> ListReturns(CancellationToken cancellationToken) => Ok(ApiResponse<IReadOnlyList<ShippingReturnResponse>>.Ok(await _shippingService.ListReturnsAsync(cancellationToken)));

    [HttpPost("returns")]
    public async Task<ActionResult<ApiResponse<ShippingReturnResponse>>> CreateReturn(ShippingReturnRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<ShippingReturnResponse>.Ok(await _shippingService.CreateReturnAsync(request, cancellationToken), "Shipping return created"));

    [HttpPut("returns/{id}")]
    public async Task<ActionResult<ApiResponse<ShippingReturnResponse>>> UpdateReturn(string id, ShippingReturnRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<ShippingReturnResponse>.Ok(await _shippingService.UpdateReturnAsync(id, request, cancellationToken), "Shipping return updated"));

    [HttpDelete("returns/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteReturn(string id, CancellationToken cancellationToken)
    {
        await _shippingService.DeleteReturnAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Shipping return deleted"));
    }
}
