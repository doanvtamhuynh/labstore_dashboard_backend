using backend.src.DTOs;
using backend.src.Helpers;
using backend.src.Models;
using backend.src.Repositories;

namespace backend.src.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customers;
    private readonly ICustomerOrderRepository _customerOrders;

    public CustomerService(ICustomerRepository customers, ICustomerOrderRepository customerOrders)
    {
        _customers = customers;
        _customerOrders = customerOrders;
    }

    public async Task<(IReadOnlyList<CustomerResponse> Items, PaginationMetadata Pagination)> ListAsync(CustomerQuery query, CancellationToken cancellationToken)
    {
        var (items, total) = await _customers.ListAsync(query, cancellationToken);
        var page = Math.Max(query.Page, 1);
        var limit = Math.Clamp(query.Limit, 1, 100);
        return (items.Select(ToResponse).ToList(), new PaginationMetadata(page, limit, total));
    }

    public async Task<CustomerDetailResponse> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var customer = await GetCustomerAsync(id, cancellationToken);
        var orders = await _customerOrders.ListByCustomerIdAsync(id, cancellationToken);
        return new CustomerDetailResponse(ToResponse(customer), orders.Select(ToOrderResponse).ToList());
    }

    public async Task<CustomerResponse> UpdateStatusAsync(string id, CustomerStatusRequest request, CancellationToken cancellationToken)
    {
        var customer = await GetCustomerAsync(id, cancellationToken);
        customer.Status = request.Status;
        await _customers.UpdateAsync(customer, cancellationToken);
        return ToResponse(customer);
    }

    public async Task<CustomerResponse> UpdateSegmentAsync(string id, CustomerSegmentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Segment))
        {
            throw new InvalidOperationException("Customer segment is required");
        }

        var customer = await GetCustomerAsync(id, cancellationToken);
        customer.Segment = request.Segment.Trim();
        await _customers.UpdateAsync(customer, cancellationToken);
        return ToResponse(customer);
    }

    public async Task<CustomerResponse> AddNoteAsync(string id, CustomerNoteRequest request, string? createdBy, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new InvalidOperationException("Note content is required");
        }

        var customer = await GetCustomerAsync(id, cancellationToken);
        customer.Notes.Add(new CustomerNote { Content = request.Content.Trim(), CreatedBy = createdBy });
        await _customers.UpdateAsync(customer, cancellationToken);
        return ToResponse(customer);
    }

    public async Task<CustomerResponse> UpdateLoyaltyAsync(string id, CustomerLoyaltyRequest request, CancellationToken cancellationToken)
    {
        if (request.Points < 0)
        {
            throw new InvalidOperationException("Loyalty points must be positive");
        }

        var customer = await GetCustomerAsync(id, cancellationToken);
        customer.LoyaltyPoints = request.Points;
        await _customers.UpdateAsync(customer, cancellationToken);
        return ToResponse(customer);
    }

    private async Task<Customer> GetCustomerAsync(string id, CancellationToken cancellationToken)
    {
        return await _customers.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Customer not found");
    }

    private static CustomerResponse ToResponse(Customer customer)
    {
        return new CustomerResponse(
            customer.Id!,
            customer.FullName,
            customer.Email,
            customer.Phone,
            customer.Status,
            customer.Segment,
            customer.LoyaltyPoints,
            customer.Notes.OrderByDescending(note => note.CreatedAtUtc).Select(note => new CustomerNoteResponse(note.Id, note.Content, note.CreatedBy, note.CreatedAtUtc)).ToList(),
            customer.CreatedAtUtc,
            customer.UpdatedAtUtc);
    }

    private static OrderResponse ToOrderResponse(Order order)
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
            new AddressResponse(order.ShippingAddress.FullName, order.ShippingAddress.Phone, order.ShippingAddress.Line1, order.ShippingAddress.Ward, order.ShippingAddress.District, order.ShippingAddress.Province, order.ShippingAddress.Country),
            order.Items.Select(item => new OrderItemResponse(item.ProductId, item.ProductName, item.Sku, item.Quantity, item.Price, item.Quantity * item.Price)).ToList(),
            order.History.Select(history => new OrderHistoryResponse(history.Id, history.FromStatus, history.ToStatus, history.Note, history.ChangedBy, history.ChangedAtUtc)).ToList(),
            order.CreatedAtUtc,
            order.UpdatedAtUtc);
    }
}
