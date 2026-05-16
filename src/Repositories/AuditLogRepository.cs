using backend.src.Models;
using MongoDB.Driver;

namespace backend.src.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly IMongoCollection<AuditLog> _collection;

    public AuditLogRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<AuditLog>("audit_logs");
    }

    public Task CreateAsync(AuditLog auditLog, CancellationToken cancellationToken)
    {
        return _collection.InsertOneAsync(auditLog, cancellationToken: cancellationToken);
    }
}
