using backend.src.Models;

namespace backend.src.DTOs;

public sealed record OrderQuery(
    OrderStatus? Status,
    string? Customer,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page = 1,
    int Limit = 20);

public sealed record UpdateOrderStatusRequest(OrderStatus Status, string? Note);

public sealed record AddressResponse(
    string FullName,
    string Phone,
    string Line1,
    string? Ward,
    string? District,
    string? Province,
    string? Country);

public sealed record OrderItemResponse(
    string ProductId,
    string ProductName,
    string Sku,
    int Quantity,
    decimal Price,
    decimal LineTotal);

public sealed record OrderHistoryResponse(
    string Id,
    OrderStatus FromStatus,
    OrderStatus ToStatus,
    string? Note,
    string? ChangedBy,
    DateTime ChangedAtUtc);

public sealed record OrderResponse(
    string Id,
    string Code,
    string CustomerId,
    string CustomerName,
    string CustomerEmail,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    string PaymentMethod,
    decimal Subtotal,
    decimal ShippingFee,
    decimal Discount,
    decimal TotalAmount,
    AddressResponse ShippingAddress,
    IReadOnlyList<OrderItemResponse> Items,
    IReadOnlyList<OrderHistoryResponse> History,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
