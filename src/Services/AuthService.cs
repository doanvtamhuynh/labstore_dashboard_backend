using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using backend.src.Config;
using backend.src.DTOs;
using backend.src.Models;
using backend.src.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace backend.src.Services;

public sealed class AuthService : IAuthService
{
    private readonly IAdminUserRepository _adminUsers;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IAuditLogRepository _auditLogs;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        IAdminUserRepository adminUsers,
        IRefreshTokenRepository refreshTokens,
        IAuditLogRepository auditLogs,
        IOptions<JwtOptions> jwtOptions)
    {
        _adminUsers = adminUsers;
        _refreshTokens = refreshTokens;
        _auditLogs = auditLogs;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken cancellationToken)
    {
        await EnsureSeedAdminAsync(cancellationToken);

        var user = await _adminUsers.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (user.IsTwoFactorEnabled || !string.IsNullOrWhiteSpace(user.TwoFactorSecret))
        {
            user.IsTwoFactorEnabled = false;
            user.TwoFactorSecret = null;
            await _adminUsers.UpdateAsync(user, cancellationToken);
        }

        await WriteAuditAsync(user.Id, "auth.login", ipAddress, cancellationToken);
        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress, CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var storedToken = await _refreshTokens.GetActiveByHashAsync(tokenHash, cancellationToken);
        if (storedToken is null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        var user = await _adminUsers.GetByIdAsync(storedToken.AdminUserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        await _refreshTokens.RevokeAsync(storedToken.Id!, cancellationToken);
        await WriteAuditAsync(user.Id, "auth.refresh-token", ipAddress, cancellationToken);
        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    public async Task LogoutAsync(LogoutRequest request, string? ipAddress, CancellationToken cancellationToken)
    {
        var storedToken = await _refreshTokens.GetActiveByHashAsync(HashToken(request.RefreshToken), cancellationToken);
        if (storedToken is null)
        {
            return;
        }

        await _refreshTokens.RevokeAsync(storedToken.Id!, cancellationToken);
        await WriteAuditAsync(storedToken.AdminUserId, "auth.logout", ipAddress, cancellationToken);
    }

    public async Task ChangePasswordAsync(string adminUserId, ChangePasswordRequest request, string? ipAddress, CancellationToken cancellationToken)
    {
        var user = await GetActiveUserAsync(adminUserId, cancellationToken);
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Current password is incorrect");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _adminUsers.UpdateAsync(user, cancellationToken);
        await WriteAuditAsync(user.Id, "auth.change-password", ipAddress, cancellationToken);
    }

    private async Task<AdminUser> GetActiveUserAsync(string adminUserId, CancellationToken cancellationToken)
    {
        var user = await _adminUsers.GetByIdAsync(adminUserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Admin user not found");
        }

        return user;
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(AdminUser user, CancellationToken cancellationToken)
    {
        var accessExpires = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        await _refreshTokens.CreateAsync(new RefreshToken
        {
            AdminUserId = user.Id!,
            TokenHash = HashToken(refreshToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
        }, cancellationToken);

        return new AuthResponse(
            GenerateAccessToken(user, accessExpires),
            refreshToken,
            accessExpires,
            ToResponse(user));
    }

    private string GenerateAccessToken(AdminUser user, DateTime expiresAtUtc)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            Expires = expiresAtUtc,
            SigningCredentials = credentials,
            Claims = new Dictionary<string, object>
            {
                [ClaimTypes.NameIdentifier] = user.Id!,
                [ClaimTypes.Email] = user.Email,
                [ClaimTypes.Role] = user.Role.ToString(),
                ["fullName"] = user.FullName
            }
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    private static AdminUserResponse ToResponse(AdminUser user)
    {
        return new AdminUserResponse(user.Id!, user.Email, user.FullName, user.Role.ToString());
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private async Task EnsureSeedAdminAsync(CancellationToken cancellationToken)
    {
        if (await _adminUsers.CountAsync(cancellationToken) > 0)
        {
            return;
        }

        var email = Environment.GetEnvironmentVariable("SEED_ADMIN_EMAIL") ?? "admin@labstore.local";
        var password = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD") ?? "Admin@123456";
        await _adminUsers.CreateAsync(new AdminUser
        {
            Email = email.Trim().ToLowerInvariant(),
            FullName = "Labstore Super Admin",
            Role = AdminRole.SuperAdmin,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        }, cancellationToken);
    }

    private Task WriteAuditAsync(string? adminUserId, string action, string? ipAddress, CancellationToken cancellationToken)
    {
        return _auditLogs.CreateAsync(new AuditLog
        {
            AdminUserId = adminUserId,
            Action = action,
            IpAddress = ipAddress
        }, cancellationToken);
    }
}
