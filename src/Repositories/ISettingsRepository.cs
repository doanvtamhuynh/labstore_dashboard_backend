using backend.src.Models;

namespace backend.src.Repositories;

public interface ISettingsRepository
{
    Task<StoreSettings> GetStoreAsync(CancellationToken cancellationToken);
    Task<StoreSettings> SaveStoreAsync(StoreSettings settings, CancellationToken cancellationToken);
    Task<GeneralSettings> GetGeneralAsync(CancellationToken cancellationToken);
    Task<GeneralSettings> SaveGeneralAsync(GeneralSettings settings, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminUser>> ListAdminsAsync(CancellationToken cancellationToken);
    Task<AdminUser?> GetAdminAsync(string id, CancellationToken cancellationToken);
    Task CreateAdminAsync(AdminUser admin, CancellationToken cancellationToken);
    Task UpdateAdminAsync(AdminUser admin, CancellationToken cancellationToken);
    Task DeleteAdminAsync(string id, CancellationToken cancellationToken);
    Task<IReadOnlyList<AuditLog>> ListAuditLogsAsync(CancellationToken cancellationToken);
}
