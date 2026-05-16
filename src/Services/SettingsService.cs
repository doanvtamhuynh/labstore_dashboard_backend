using System.IO.Compression;
using backend.src.DTOs;
using backend.src.Config;
using backend.src.Models;
using backend.src.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace backend.src.Services;

public sealed class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _settings;
    private readonly IMongoDatabase _database;
    private readonly BackupOptions _backupOptions;

    public SettingsService(ISettingsRepository settings, IMongoDatabase database, IOptions<BackupOptions> backupOptions)
    {
        _settings = settings;
        _database = database;
        _backupOptions = backupOptions.Value;
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

    public async Task<BackupResponse> BackupAsync(CancellationToken cancellationToken)
    {
        var createdAt = DateTime.UtcNow;
        var id = ObjectId.GenerateNewId().ToString();
        var databaseName = _database.DatabaseNamespace.DatabaseName;
        var backupRoot = Path.IsPathRooted(_backupOptions.Directory)
            ? _backupOptions.Directory
            : Path.Combine(Directory.GetCurrentDirectory(), _backupOptions.Directory);
        Directory.CreateDirectory(backupRoot);

        var filePath = Path.Combine(backupRoot, $"{databaseName}-{createdAt:yyyyMMdd-HHmmss}-{id}.zip");
        await using (var file = File.Create(filePath))
        using (var archive = new ZipArchive(file, ZipArchiveMode.Create))
        {
            var collectionNames = await (await _database.ListCollectionNamesAsync(cancellationToken: cancellationToken)).ToListAsync(cancellationToken);
            foreach (var collectionName in collectionNames.OrderBy(name => name, StringComparer.OrdinalIgnoreCase))
            {
                var entry = archive.CreateEntry($"{collectionName}.jsonl", CompressionLevel.Optimal);
                await using var entryStream = entry.Open();
                await using var writer = new StreamWriter(entryStream);
                var collection = _database.GetCollection<BsonDocument>(collectionName);
                using var cursor = await collection.Find(Builders<BsonDocument>.Filter.Empty).ToCursorAsync(cancellationToken);
                while (await cursor.MoveNextAsync(cancellationToken))
                {
                    foreach (var document in cursor.Current)
                    {
                        await writer.WriteLineAsync(document.ToJson());
                    }
                }
            }
        }

        var size = new FileInfo(filePath).Length;
        return new BackupResponse(id, databaseName, createdAt, "completed", filePath, size);
    }

    private static AdminUserResponse ToResponse(AdminUser admin) => new(admin.Id!, admin.Email, admin.FullName, admin.Role.ToString());
}
