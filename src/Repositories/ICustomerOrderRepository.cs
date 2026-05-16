using backend.src.Models;

namespace backend.src.Repositories;

public interface ICustomerOrderRepository
{
    Task<IReadOnlyList<Order>> ListByCustomerIdAsync(string customerId, CancellationToken cancellationToken);
}
