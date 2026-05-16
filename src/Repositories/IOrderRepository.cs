using backend.src.DTOs;
using backend.src.Models;

namespace backend.src.Repositories;

public interface IOrderRepository
{
    Task<(IReadOnlyList<Order> Items, long Total)> ListAsync(OrderQuery query, CancellationToken cancellationToken);
    Task<Order?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task UpdateAsync(Order order, CancellationToken cancellationToken);
}
