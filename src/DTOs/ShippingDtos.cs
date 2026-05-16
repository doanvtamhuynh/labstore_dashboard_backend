using backend.src.Models;

namespace backend.src.DTOs;

public sealed record ShippingConfigRequest(string Region, decimal MinWeightKg, decimal MaxWeightKg, decimal Fee, bool IsActive);

public sealed record ShippingConfigResponse(string Id, string Region, decimal MinWeightKg, decimal MaxWeightKg, decimal Fee, bool IsActive);

public sealed record ShippingProviderResponse(string Id, string Name, string Code, string? TrackingUrlTemplate, bool IsIntegrated);

public sealed record WarehouseRequest(string Name, string Address, string Province, string? Phone, bool IsDefault);

public sealed record WarehouseResponse(string Id, string Name, string Address, string Province, string? Phone, bool IsDefault);

public sealed record ShippingReturnRequest(string OrderId, string CustomerId, string Reason, ShippingReturnStatus Status, string? ResolutionNote);

public sealed record ShippingReturnResponse(string Id, string OrderId, string CustomerId, string Reason, ShippingReturnStatus Status, string? ResolutionNote, DateTime CreatedAtUtc);

public sealed record ShippingTrackingResponse(string OrderId, string Status, string? Provider, string? TrackingCode, string? TrackingUrl, DateTime UpdatedAtUtc);
