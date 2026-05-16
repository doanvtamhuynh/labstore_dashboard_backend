using backend.src.Models;
using MongoDB.Driver;

namespace backend.src.Repositories;

public sealed class SettingsRepository : ISettingsRepository
{
    private readonly IMongoCollection<StoreSettings> _store;
    private readonly IMongoCollection<GeneralSettings> _general;
    private readonly IMongoCollection<AdminUser> _admins;
    private readonly IMongoCollection<AuditLog> _auditLogs;

    public SettingsRepository(IMongoDatabase database)
    {
        _store = database.GetCollection<StoreSettings>("settings_store");
        _general = database.GetCollection<GeneralSettings>("settings_general");
        _admins = database.GetCollection<AdminUser>("admin_users");
        _auditLogs = database.GetCollection<AuditLog>("audit_logs");
    }

    public async Task<StoreSettings> GetStoreAsync(CancellationToken cancellationToken) => await _store.Find(Builders<StoreSettings>.Filter.Empty).FirstOrDefaultAsync(cancellationToken) ?? new StoreSettings();
    public async Task<StoreSettings> SaveStoreAsync(StoreSettings settings, CancellationToken cancellationToken)
    {
        if (settings.Id is null) await _store.InsertOneAsync(settings, cancellationToken: cancellationToken);
        else await _store.ReplaceOneAsync(item => item.Id == settings.Id, settings, cancellationToken: cancellationToken);
        return settings;
    }
    public async Task<GeneralSettings> GetGeneralAsync(CancellationToken cancellationToken) => await _general.Find(Builders<GeneralSettings>.Filter.Empty).FirstOrDefaultAsync(cancellationToken) ?? new GeneralSettings();
    public async Task<GeneralSettings> SaveGeneralAsync(GeneralSettings settings, CancellationToken cancellationToken)
    {
        if (settings.Id is null) await _general.InsertOneAsync(settings, cancellationToken: cancellationToken);
        else await _general.ReplaceOneAsync(item => item.Id == settings.Id, settings, cancellationToken: cancellationToken);
        return settings;
    }
    public async Task<IReadOnlyList<AdminUser>> ListAdminsAsync(CancellationToken cancellationToken) => await _admins.Find(Builders<AdminUser>.Filter.Empty).ToListAsync(cancellationToken);
    public async Task<AdminUser?> GetAdminAsync(string id, CancellationToken cancellationToken) => await _admins.Find(admin => admin.Id == id).FirstOrDefaultAsync(cancellationToken);
    public Task CreateAdminAsync(AdminUser admin, CancellationToken cancellationToken) => _admins.InsertOneAsync(admin, cancellationToken: cancellationToken);
    public Task UpdateAdminAsync(AdminUser admin, CancellationToken cancellationToken) => _admins.ReplaceOneAsync(item => item.Id == admin.Id, admin, cancellationToken: cancellationToken);
    public Task DeleteAdminAsync(string id, CancellationToken cancellationToken) => _admins.DeleteOneAsync(admin => admin.Id == id, cancellationToken);
    public async Task<IReadOnlyList<AuditLog>> ListAuditLogsAsync(CancellationToken cancellationToken) => await _auditLogs.Find(Builders<AuditLog>.Filter.Empty).SortByDescending(log => log.CreatedAtUtc).Limit(200).ToListAsync(cancellationToken);
}
