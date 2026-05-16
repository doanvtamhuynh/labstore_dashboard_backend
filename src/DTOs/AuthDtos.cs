namespace backend.src.DTOs;

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record LogoutRequest(string RefreshToken);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed record AdminUserResponse(string Id, string Email, string FullName, string Role);

public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc, AdminUserResponse User);
