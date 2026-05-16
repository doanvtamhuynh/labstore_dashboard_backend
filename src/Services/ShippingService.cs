using backend.src.DTOs;
using backend.src.Models;
using backend.src.Repositories;

namespace backend.src.Services;

public sealed class ShippingService : IShippingService
{
    private readonly ICrudRepository<ShippingConfig> _configs;
    private readonly ICrudRepository<ShippingProvider> _providers;
    private readonly ICrudRepository<Warehouse> _warehouses;
    private readonly ICrudRepository<ShippingReturn> _returns;
    private readonly IOrderRepository _orders;

    public ShippingService(
        ICrudRepository<ShippingConfig> configs,
        ICrudRepository<ShippingProvider> providers,
        ICrudRepository<Warehouse> warehouses,
        ICrudRepository<ShippingReturn> returns,
        IOrderRepository orders)
    {
        _configs = configs;
        _providers = providers;
        _warehouses = warehouses;
        _returns = returns;
        _orders = orders;
    }

    public async Task<IReadOnlyList<ShippingConfigResponse>> ListConfigsAsync(CancellationToken cancellationToken)
    {
        return (await _configs.ListAsync(cancellationToken)).Select(ToResponse).ToList();
    }

    public async Task<ShippingConfigResponse> CreateConfigAsync(ShippingConfigRequest request, CancellationToken cancellationToken)
    {
        ValidateConfig(request);
        var item = new ShippingConfig { Region = request.Region.Trim(), MinWeightKg = request.MinWeightKg, MaxWeightKg = request.MaxWeightKg, Fee = request.Fee, IsActive = request.IsActive };
        await _configs.CreateAsync(item, cancellationToken);
        return ToResponse(item);
    }

    public async Task<ShippingConfigResponse> UpdateConfigAsync(string id, ShippingConfigRequest request, CancellationToken cancellationToken)
    {
        ValidateConfig(request);
        var item = await RequireAsync(_configs, id, cancellationToken);
        item.Region = request.Region.Trim();
        item.MinWeightKg = request.MinWeightKg;
        item.MaxWeightKg = request.MaxWeightKg;
        item.Fee = request.Fee;
        item.IsActive = request.IsActive;
        item.UpdatedAtUtc = DateTime.UtcNow;
        await _configs.UpdateAsync(id, item, cancellationToken);
        return ToResponse(item);
    }

    public Task DeleteConfigAsync(string id, CancellationToken cancellationToken) => _configs.DeleteAsync(id, cancellationToken);

    public async Task<IReadOnlyList<ShippingProviderResponse>> ListProvidersAsync(CancellationToken cancellationToken)
    {
        var providers = await _providers.ListAsync(cancellationToken);
        if (providers.Count == 0)
        {
            return DefaultProviders();
        }

        return providers.Select(ToResponse).ToList();
    }

    public async Task<ShippingTrackingResponse> GetTrackingAsync(string orderId, CancellationToken cancellationToken)
    {
        var order = await _orders.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            throw new InvalidOperationException("Order not found");
        }

        return new ShippingTrackingResponse(
            orderId,
            order.Status.ToString(),
            null,
            null,
            null,
            order.UpdatedAtUtc);
    }

    public async Task<IReadOnlyList<WarehouseResponse>> ListWarehousesAsync(CancellationToken cancellationToken)
    {
        return (await _warehouses.ListAsync(cancellationToken)).Select(ToResponse).ToList();
    }

    public async Task<WarehouseResponse> CreateWarehouseAsync(WarehouseRequest request, CancellationToken cancellationToken)
    {
        ValidateWarehouse(request);
        var item = new Warehouse { Name = request.Name.Trim(), Address = request.Address.Trim(), Province = request.Province.Trim(), Phone = request.Phone, IsDefault = request.IsDefault };
        await _warehouses.CreateAsync(item, cancellationToken);
        return ToResponse(item);
    }

    public async Task<WarehouseResponse> UpdateWarehouseAsync(string id, WarehouseRequest request, CancellationToken cancellationToken)
    {
        ValidateWarehouse(request);
        var item = await RequireAsync(_warehouses, id, cancellationToken);
        item.Name = request.Name.Trim();
        item.Address = request.Address.Trim();
        item.Province = request.Province.Trim();
        item.Phone = request.Phone;
        item.IsDefault = request.IsDefault;
        item.UpdatedAtUtc = DateTime.UtcNow;
        await _warehouses.UpdateAsync(id, item, cancellationToken);
        return ToResponse(item);
    }

    public Task DeleteWarehouseAsync(string id, CancellationToken cancellationToken) => _warehouses.DeleteAsync(id, cancellationToken);

    public async Task<IReadOnlyList<ShippingReturnResponse>> ListReturnsAsync(CancellationToken cancellationToken)
    {
        return (await _returns.ListAsync(cancellationToken)).Select(ToResponse).ToList();
    }

    public async Task<ShippingReturnResponse> CreateReturnAsync(ShippingReturnRequest request, CancellationToken cancellationToken)
    {
        ValidateReturn(request);
        var item = new ShippingReturn { OrderId = request.OrderId, CustomerId = request.CustomerId, Reason = request.Reason.Trim(), Status = request.Status, ResolutionNote = request.ResolutionNote };
        await _returns.CreateAsync(item, cancellationToken);
        return ToResponse(item);
    }

    public async Task<ShippingReturnResponse> UpdateReturnAsync(string id, ShippingReturnRequest request, CancellationToken cancellationToken)
    {
        ValidateReturn(request);
        var item = await RequireAsync(_returns, id, cancellationToken);
        item.OrderId = request.OrderId;
        item.CustomerId = request.CustomerId;
        item.Reason = request.Reason.Trim();
        item.Status = request.Status;
        item.ResolutionNote = request.ResolutionNote;
        item.UpdatedAtUtc = DateTime.UtcNow;
        await _returns.UpdateAsync(id, item, cancellationToken);
        return ToResponse(item);
    }

    public Task DeleteReturnAsync(string id, CancellationToken cancellationToken) => _returns.DeleteAsync(id, cancellationToken);

    private static async Task<T> RequireAsync<T>(ICrudRepository<T> repository, string id, CancellationToken cancellationToken) where T : class
    {
        return await repository.GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Shipping item not found");
    }

    private static void ValidateConfig(ShippingConfigRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Region) || request.MinWeightKg < 0 || request.MaxWeightKg < request.MinWeightKg || request.Fee < 0)
        {
            throw new InvalidOperationException("Shipping config is invalid");
        }
    }

    private static void ValidateWarehouse(WarehouseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Address) || string.IsNullOrWhiteSpace(request.Province))
        {
            throw new InvalidOperationException("Warehouse name, address, and province are required");
        }
    }

    private static void ValidateReturn(ShippingReturnRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.OrderId) || string.IsNullOrWhiteSpace(request.CustomerId) || string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new InvalidOperationException("Return order, customer, and reason are required");
        }
    }

    private static IReadOnlyList<ShippingProviderResponse> DefaultProviders()
    {
        return new[]
        {
            new ShippingProviderResponse("default-ghn", "Giao Hang Nhanh", "GHN", "https://ghn.vn/tracking?order_code={trackingCode}", false),
            new ShippingProviderResponse("default-ghtk", "Giao Hang Tiet Kiem", "GHTK", "https://i.ghtk.vn/{trackingCode}", false),
            new ShippingProviderResponse("default-viettelpost", "Viettel Post", "VTP", "https://viettelpost.com.vn/tra-cuu-hanh-trinh-don/?order={trackingCode}", false)
        };
    }

    private static ShippingConfigResponse ToResponse(ShippingConfig item) => new(item.Id!, item.Region, item.MinWeightKg, item.MaxWeightKg, item.Fee, item.IsActive);
    private static ShippingProviderResponse ToResponse(ShippingProvider item) => new(item.Id!, item.Name, item.Code, item.TrackingUrlTemplate, item.IsIntegrated);
    private static WarehouseResponse ToResponse(Warehouse item) => new(item.Id!, item.Name, item.Address, item.Province, item.Phone, item.IsDefault);
    private static ShippingReturnResponse ToResponse(ShippingReturn item) => new(item.Id!, item.OrderId, item.CustomerId, item.Reason, item.Status, item.ResolutionNote, item.CreatedAtUtc);
}
