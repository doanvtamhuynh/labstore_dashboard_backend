using backend.src.DTOs;
using backend.src.Models;
using backend.src.Repositories;
using MongoDB.Bson;

namespace backend.src.Services;

public sealed class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _settings;

    public SettingsService(ISettingsRepository settings)
    {
        _settings = settings;
    }

    public async Task<StoreSettingsRequest> GetStoreAsync(CancellationToken cancellationToken)
    {
        var item = await _settings.GetStoreAsync(cancellationToken);
        return new StoreSettingsRequest(item.StoreName, item.LogoUrl, item.Address, item.TimeZone);
    }

    public async Task<StoreSettingsRequest> UpdateStoreAsync(StoreSettingsRequest request, CancellationToken cancellationToken)
    {
        var item = await _settings.GetStoreAsync(cancellationToken);
        item.StoreName = request.StoreName; item.LogoUrl = request.LogoUrl; item.Address = request.Address; item.TimeZone = request.TimeZone;
        await _settings.SaveStoreAsync(item, cancellationToken);
        return request;
    }

    public async Task<GeneralSettingsRequest> GetGeneralAsync(CancellationToken cancellationToken)
    {
        var item = await _settings.GetGeneralAsync(cancellationToken);
        return new GeneralSettingsRequest(item.TaxRate, item.Currency, item.Language);
    }

    public async Task<GeneralSettingsRequest> UpdateGeneralAsync(GeneralSettingsRequest request, CancellationToken cancellationToken)
    {
        var item = await _settings.GetGeneralAsync(cancellationToken);
        item.TaxRate = request.TaxRate; item.Currency = request.Currency; item.Language = request.Language;
        await _settings.SaveGeneralAsync(item, cancellationToken);
        return request;
    }

    public async Task<IReadOnlyList<AdminUserResponse>> ListAdminsAsync(CancellationToken cancellationToken) => (await _settings.ListAdminsAsync(cancellationToken)).Select(ToResponse).ToList();
    public async Task<AdminUserResponse> CreateAdminAsync(AdminCreateRequest request, CancellationToken cancellationToken)
    {
        var admin = new AdminUser { Email = request.Email.Trim().ToLowerInvariant(), FullName = request.FullName.Trim(), PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), Role = request.Role, IsActive = request.IsActive };
        await _settings.CreateAdminAsync(admin, cancellationToken);
        return ToResponse(admin);
    }
    public async Task<AdminUserResponse> UpdateAdminAsync(string id, AdminUpdateRequest request, CancellationToken cancellationToken)
    {
        var admin = await _settings.GetAdminAsync(id, cancellationToken) ?? throw new InvalidOperationException("Admin not found");
        admin.Email = request.Email.Trim().ToLowerInvariant(); admin.FullName = request.FullName.Trim(); admin.Role = request.Role; admin.IsActive = request.IsActive; admin.UpdatedAtUtc = DateTime.UtcNow;
        await _settings.UpdateAdminAsync(admin, cancellationToken);
        return ToResponse(admin);
    }
    public Task DeleteAdminAsync(string id, CancellationToken cancellationToken) => _settings.DeleteAdminAsync(id, cancellationToken);
    public async Task<IReadOnlyList<AuditLogResponse>> ListAuditLogsAsync(CancellationToken cancellationToken) => (await _settings.ListAuditLogsAsync(cancellationToken)).Select(log => new AuditLogResponse(log.Id!, log.AdminUserId, log.Action, log.IpAddress, log.CreatedAtUtc)).ToList();
    public Task<BackupResponse> BackupAsync(CancellationToken cancellationToken) => Task.FromResult(new BackupResponse(ObjectId.GenerateNewId().ToString(), "labstore", DateTime.UtcNow, "queued"));
    private static AdminUserResponse ToResponse(AdminUser admin) => new(admin.Id!, admin.Email, admin.FullName, admin.Role, admin.IsTwoFactorEnabled);
}
