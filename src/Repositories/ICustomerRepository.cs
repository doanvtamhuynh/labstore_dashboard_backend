using backend.src.DTOs;
using backend.src.Models;

namespace backend.src.Repositories;

public interface ICustomerRepository
{
    Task<(IReadOnlyList<Customer> Items, long Total)> ListAsync(CustomerQuery query, CancellationToken cancellationToken);
    Task<Customer?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken);
}
