using backend.src.DTOs;

namespace backend.src.Services;

public interface ISettingsService
{
    Task<StoreSettingsRequest> GetStoreAsync(CancellationToken cancellationToken);
    Task<StoreSettingsRequest> UpdateStoreAsync(StoreSettingsRequest request, CancellationToken cancellationToken);
    Task<GeneralSettingsRequest> GetGeneralAsync(CancellationToken cancellationToken);
    Task<GeneralSettingsRequest> UpdateGeneralAsync(GeneralSettingsRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminUserResponse>> ListAdminsAsync(CancellationToken cancellationToken);
    Task<AdminUserResponse> CreateAdminAsync(AdminCreateRequest request, CancellationToken cancellationToken);
    Task<AdminUserResponse> UpdateAdminAsync(string id, AdminUpdateRequest request, CancellationToken cancellationToken);
    Task DeleteAdminAsync(string id, CancellationToken cancellationToken);
    Task<IReadOnlyList<AuditLogResponse>> ListAuditLogsAsync(CancellationToken cancellationToken);
    Task<BackupResponse> BackupAsync(CancellationToken cancellationToken);
}
