using backend.src.Models;

namespace backend.src.Repositories;

public interface IAuditLogRepository
{
    Task CreateAsync(AuditLog auditLog, CancellationToken cancellationToken);
}
