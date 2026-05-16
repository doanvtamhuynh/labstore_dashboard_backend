using backend.src.DTOs;

namespace backend.src.Services;

public interface IShippingService
{
    Task<IReadOnlyList<ShippingConfigResponse>> ListConfigsAsync(CancellationToken cancellationToken);
    Task<ShippingConfigResponse> CreateConfigAsync(ShippingConfigRequest request, CancellationToken cancellationToken);
    Task<ShippingConfigResponse> UpdateConfigAsync(string id, ShippingConfigRequest request, CancellationToken cancellationToken);
    Task DeleteConfigAsync(string id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ShippingProviderResponse>> ListProvidersAsync(CancellationToken cancellationToken);
    Task<ShippingTrackingResponse> GetTrackingAsync(string orderId, CancellationToken cancellationToken);
    Task<IReadOnlyList<WarehouseResponse>> ListWarehousesAsync(CancellationToken cancellationToken);
    Task<WarehouseResponse> CreateWarehouseAsync(WarehouseRequest request, CancellationToken cancellationToken);
    Task<WarehouseResponse> UpdateWarehouseAsync(string id, WarehouseRequest request, CancellationToken cancellationToken);
    Task DeleteWarehouseAsync(string id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ShippingReturnResponse>> ListReturnsAsync(CancellationToken cancellationToken);
    Task<ShippingReturnResponse> CreateReturnAsync(ShippingReturnRequest request, CancellationToken cancellationToken);
    Task<ShippingReturnResponse> UpdateReturnAsync(string id, ShippingReturnRequest request, CancellationToken cancellationToken);
    Task DeleteReturnAsync(string id, CancellationToken cancellationToken);
}
