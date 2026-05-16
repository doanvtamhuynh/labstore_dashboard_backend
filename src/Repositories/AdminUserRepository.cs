using backend.src.Models;
using MongoDB.Driver;

namespace backend.src.Repositories;

public sealed class AdminUserRepository : IAdminUserRepository
{
    private readonly IMongoCollection<AdminUser> _collection;

    public AdminUserRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<AdminUser>("admin_users");
    }

    public async Task<AdminUser?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return await _collection.Find(user => user.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AdminUser?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await _collection.Find(user => user.Email == normalizedEmail).FirstOrDefaultAsync(cancellationToken);
    }

    public Task CreateAsync(AdminUser adminUser, CancellationToken cancellationToken)
    {
        return _collection.InsertOneAsync(adminUser, cancellationToken: cancellationToken);
    }

    public Task UpdateAsync(AdminUser adminUser, CancellationToken cancellationToken)
    {
        adminUser.UpdatedAtUtc = DateTime.UtcNow;
        return _collection.ReplaceOneAsync(user => user.Id == adminUser.Id, adminUser, cancellationToken: cancellationToken);
    }

    public Task<long> CountAsync(CancellationToken cancellationToken)
    {
        return _collection.CountDocumentsAsync(Builders<AdminUser>.Filter.Empty, cancellationToken: cancellationToken);
    }
}
