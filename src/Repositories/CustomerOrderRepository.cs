using backend.src.Models;
using MongoDB.Driver;

namespace backend.src.Repositories;

public sealed class CustomerOrderRepository : ICustomerOrderRepository
{
    private readonly IMongoCollection<Order> _orders;

    public CustomerOrderRepository(IMongoDatabase database)
    {
        _orders = database.GetCollection<Order>("orders");
    }

    public async Task<IReadOnlyList<Order>> ListByCustomerIdAsync(string customerId, CancellationToken cancellationToken)
    {
        return await _orders.Find(order => order.CustomerId == customerId)
            .SortByDescending(order => order.CreatedAtUtc)
            .Limit(50)
            .ToListAsync(cancellationToken);
    }
}
