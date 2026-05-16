using backend.src.Models;

namespace backend.src.DTOs;

public sealed record StoreSettingsRequest(string StoreName, string? LogoUrl, string Address, string TimeZone);
public sealed record GeneralSettingsRequest(decimal TaxRate, string Currency, string Language);
public sealed record AdminCreateRequest(string Email, string FullName, string Password, AdminRole Role, bool IsActive);
public sealed record AdminUpdateRequest(string Email, string FullName, AdminRole Role, bool IsActive);
public sealed record AuditLogResponse(string Id, string? AdminUserId, string Action, string? IpAddress, DateTime CreatedAtUtc);
public sealed record BackupResponse(string Id, string DatabaseName, DateTime CreatedAtUtc, string Status);
