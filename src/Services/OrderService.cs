using System.Globalization;
using System.Text;
using backend.src.DTOs;
using backend.src.Helpers;
using backend.src.Models;
using backend.src.Repositories;

namespace backend.src.Services;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orders;

    public OrderService(IOrderRepository orders)
    {
        _orders = orders;
    }

    public async Task<(IReadOnlyList<OrderResponse> Items, PaginationMetadata Pagination)> ListAsync(OrderQuery query, CancellationToken cancellationToken)
    {
        var (items, total) = await _orders.ListAsync(query, cancellationToken);
        var page = Math.Max(query.Page, 1);
        var limit = Math.Clamp(query.Limit, 1, 100);
        return (items.Select(ToResponse).ToList(), new PaginationMetadata(page, limit, total));
    }

    public async Task<OrderResponse> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return ToResponse(await GetOrderAsync(id, cancellationToken));
    }

    public async Task<OrderResponse> UpdateStatusAsync(string id, UpdateOrderStatusRequest request, string? changedBy, CancellationToken cancellationToken)
    {
        var order = await GetOrderAsync(id, cancellationToken);
        if (order.Status == request.Status)
        {
            return ToResponse(order);
        }

        order.History.Add(new OrderHistoryEntry
        {
            FromStatus = order.Status,
            ToStatus = request.Status,
            Note = request.Note,
            ChangedBy = changedBy
        });
        order.Status = request.Status;
        await _orders.UpdateAsync(order, cancellationToken);
        return ToResponse(order);
    }

    public async Task<IReadOnlyList<OrderHistoryResponse>> GetHistoryAsync(string id, CancellationToken cancellationToken)
    {
        var order = await GetOrderAsync(id, cancellationToken);
        return order.History.OrderByDescending(item => item.ChangedAtUtc).Select(ToHistoryResponse).ToList();
    }

    public async Task<byte[]> CreateInvoicePdfAsync(string id, CancellationToken cancellationToken)
    {
        var order = await GetOrderAsync(id, cancellationToken);
        var content = new StringBuilder();
        content.AppendLine("%PDF-1.4");
        content.AppendLine("% Labstore invoice placeholder");
        content.AppendLine($"Invoice: {order.Code}");
        content.AppendLine($"Customer: {order.CustomerName} <{order.CustomerEmail}>");
        content.AppendLine($"Total: {order.TotalAmount.ToString(CultureInfo.InvariantCulture)}");
        content.AppendLine("%%EOF");
        return Encoding.UTF8.GetBytes(content.ToString());
    }

    public async Task<string> ExportCsvAsync(OrderQuery query, CancellationToken cancellationToken)
    {
        var exportQuery = query with { Page = 1, Limit = 1000 };
        var (items, _) = await _orders.ListAsync(exportQuery, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine("id,code,customerName,customerEmail,status,paymentStatus,totalAmount,createdAtUtc");
        foreach (var order in items)
        {
            builder.AppendLine(string.Join(',', new[]
            {
                Escape(order.Id),
                Escape(order.Code),
                Escape(order.CustomerName),
                Escape(order.CustomerEmail),
                order.Status.ToString(),
                order.PaymentStatus.ToString(),
                order.TotalAmount.ToString(CultureInfo.InvariantCulture),
                order.CreatedAtUtc.ToString("O", CultureInfo.InvariantCulture)
            }));
        }

        return builder.ToString();
    }

    private async Task<Order> GetOrderAsync(string id, CancellationToken cancellationToken)
    {
        return await _orders.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Order not found");
    }

    private static OrderResponse ToResponse(Order order)
    {
        return new OrderResponse(
            order.Id!,
            order.Code,
            order.CustomerId,
            order.CustomerName,
            order.CustomerEmail,
            order.Status,
            order.PaymentStatus,
            order.PaymentMethod,
            order.Subtotal,
            order.ShippingFee,
            order.Discount,
            order.TotalAmount,
            new AddressResponse(
                order.ShippingAddress.FullName,
                order.ShippingAddress.Phone,
                order.ShippingAddress.Line1,
                order.ShippingAddress.Ward,
                order.ShippingAddress.District,
                order.ShippingAddress.Province,
                order.ShippingAddress.Country),
            order.Items.Select(item => new OrderItemResponse(
                item.ProductId,
                item.ProductName,
                item.Sku,
                item.Quantity,
                item.Price,
                item.Quantity * item.Price)).ToList(),
            order.History.OrderByDescending(item => item.ChangedAtUtc).Select(ToHistoryResponse).ToList(),
            order.CreatedAtUtc,
            order.UpdatedAtUtc);
    }

    private static OrderHistoryResponse ToHistoryResponse(OrderHistoryEntry item)
    {
        return new OrderHistoryResponse(item.Id, item.FromStatus, item.ToStatus, item.Note, item.ChangedBy, item.ChangedAtUtc);
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Contains(',') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    }
}
