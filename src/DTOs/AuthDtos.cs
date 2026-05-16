using backend.src.Models;

namespace backend.src.DTOs;

public sealed record LoginRequest(string Email, string Password, string? TwoFactorCode);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record LogoutRequest(string RefreshToken);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed record VerifyTwoFactorRequest(string Code);

public sealed record AdminUserResponse(string Id, string Email, string FullName, AdminRole Role, bool IsTwoFactorEnabled);

public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc, AdminUserResponse User);

public sealed record EnableTwoFactorResponse(string Secret, string ManualEntryKey);
