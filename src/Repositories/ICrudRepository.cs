namespace backend.src.Repositories;

public interface ICrudRepository<T> where T : class
{
    Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken);
    Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task CreateAsync(T item, CancellationToken cancellationToken);
    Task UpdateAsync(string id, T item, CancellationToken cancellationToken);
    Task DeleteAsync(string id, CancellationToken cancellationToken);
}
