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
        var lines = new[]
        {
            "Labstore Invoice",
            $"Invoice: {order.Code}",
            $"Customer: {order.CustomerName} <{order.CustomerEmail}>",
            $"Status: {order.Status}",
            $"Total: {order.TotalAmount.ToString(CultureInfo.InvariantCulture)}"
        };
        return BuildSimplePdf(lines);
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

    private static byte[] BuildSimplePdf(IReadOnlyList<string> lines)
    {
        var stream = string.Join(Environment.NewLine, lines.Select((line, index) => $"BT /F1 12 Tf 72 {760 - (index * 20)} Td ({EscapePdf(line)}) Tj ET"));
        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}\nendstream"
        };

        var builder = new StringBuilder("%PDF-1.4\n");
        var offsets = new List<int> { 0 };
        foreach (var (body, index) in objects.Select((body, index) => (body, index)))
        {
            offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString()));
            builder.Append(index + 1).Append(" 0 obj\n").Append(body).Append("\nendobj\n");
        }

        var xrefOffset = Encoding.ASCII.GetByteCount(builder.ToString());
        builder.Append("xref\n0 ").Append(objects.Length + 1).Append('\n');
        builder.Append("0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1))
        {
            builder.Append(offset.ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
        }

        builder.Append("trailer\n<< /Size ").Append(objects.Length + 1).Append(" /Root 1 0 R >>\nstartxref\n").Append(xrefOffset).Append("\n%%EOF");
        return Encoding.ASCII.GetBytes(builder.ToString());
    }

    private static string EscapePdf(string value)
    {
        return value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
    }
}
