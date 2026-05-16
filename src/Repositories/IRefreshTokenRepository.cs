using backend.src.Models;

namespace backend.src.Repositories;

public interface IRefreshTokenRepository
{
    Task CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
    Task<RefreshToken?> GetActiveByHashAsync(string tokenHash, CancellationToken cancellationToken);
    Task RevokeAsync(string id, CancellationToken cancellationToken);
}
