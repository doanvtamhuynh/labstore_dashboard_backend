using backend.src.DTOs;

namespace backend.src.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken cancellationToken);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress, CancellationToken cancellationToken);
    Task LogoutAsync(LogoutRequest request, string? ipAddress, CancellationToken cancellationToken);
    Task ChangePasswordAsync(string adminUserId, ChangePasswordRequest request, string? ipAddress, CancellationToken cancellationToken);
}
