using backend.src.DTOs;
using backend.src.Models;

namespace backend.src.Repositories;

public interface IProductRepository
{
    Task<(IReadOnlyList<Product> Items, long Total)> ListAsync(ProductQuery query, CancellationToken cancellationToken);
    Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task CreateAsync(Product product, CancellationToken cancellationToken);
    Task UpdateAsync(Product product, CancellationToken cancellationToken);
    Task DeleteAsync(string id, CancellationToken cancellationToken);
}
