using backend.src.Models;
using MongoDB.Driver;

namespace backend.src.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IMongoCollection<RefreshToken> _collection;

    public RefreshTokenRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<RefreshToken>("refresh_tokens");
    }

    public Task CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        return _collection.InsertOneAsync(refreshToken, cancellationToken: cancellationToken);
    }

    public async Task<RefreshToken?> GetActiveByHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        var filter = Builders<RefreshToken>.Filter.And(
            Builders<RefreshToken>.Filter.Eq(token => token.TokenHash, tokenHash),
            Builders<RefreshToken>.Filter.Eq(token => token.RevokedAtUtc, null),
            Builders<RefreshToken>.Filter.Gt(token => token.ExpiresAtUtc, DateTime.UtcNow));

        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public Task RevokeAsync(string id, CancellationToken cancellationToken)
    {
        var update = Builders<RefreshToken>.Update.Set(token => token.RevokedAtUtc, DateTime.UtcNow);
        return _collection.UpdateOneAsync(token => token.Id == id, update, cancellationToken: cancellationToken);
    }
}
