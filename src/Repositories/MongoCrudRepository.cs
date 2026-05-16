using MongoDB.Driver;

namespace backend.src.Repositories;

public sealed class MongoCrudRepository<T> : ICrudRepository<T> where T : class
{
    private readonly IMongoCollection<T> _collection;

    public MongoCrudRepository(IMongoDatabase database, string collectionName)
    {
        _collection = database.GetCollection<T>(collectionName);
    }

    public async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken)
    {
        return await _collection.Find(Builders<T>.Filter.Empty).ToListAsync(cancellationToken);
    }

    public async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return await _collection.Find(Builders<T>.Filter.Eq("Id", id)).FirstOrDefaultAsync(cancellationToken);
    }

    public Task CreateAsync(T item, CancellationToken cancellationToken)
    {
        return _collection.InsertOneAsync(item, cancellationToken: cancellationToken);
    }

    public Task UpdateAsync(string id, T item, CancellationToken cancellationToken)
    {
        return _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("Id", id), item, cancellationToken: cancellationToken);
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken)
    {
        return _collection.DeleteOneAsync(Builders<T>.Filter.Eq("Id", id), cancellationToken);
    }
}
