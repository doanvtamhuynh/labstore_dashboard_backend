using backend.src.Models;
using MongoDB.Driver;

namespace backend.src.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly IMongoCollection<Category> _collection;

    public CategoryRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Category>("categories");
    }

    public async Task<IReadOnlyList<Category>> ListAsync(CancellationToken cancellationToken)
    {
        return await _collection.Find(Builders<Category>.Filter.Empty)
            .SortBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return await _collection.Find(category => category.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public Task CreateAsync(Category category, CancellationToken cancellationToken)
    {
        return _collection.InsertOneAsync(category, cancellationToken: cancellationToken);
    }

    public Task UpdateAsync(Category category, CancellationToken cancellationToken)
    {
        category.UpdatedAtUtc = DateTime.UtcNow;
        return _collection.ReplaceOneAsync(item => item.Id == category.Id, category, cancellationToken: cancellationToken);
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken)
    {
        return _collection.DeleteOneAsync(category => category.Id == id, cancellationToken);
    }

    public async Task ReorderAsync(IReadOnlyList<Category> categories, CancellationToken cancellationToken)
    {
        if (categories.Count == 0)
        {
            return;
        }

        var writes = categories.Select(category =>
            new ReplaceOneModel<Category>(Builders<Category>.Filter.Eq(item => item.Id, category.Id), category)).ToList();
        await _collection.BulkWriteAsync(writes, cancellationToken: cancellationToken);
    }
}
