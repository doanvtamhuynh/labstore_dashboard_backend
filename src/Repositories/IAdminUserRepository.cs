using backend.src.Models;

namespace backend.src.Repositories;

public interface IAdminUserRepository
{
    Task<AdminUser?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<AdminUser?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task CreateAsync(AdminUser adminUser, CancellationToken cancellationToken);
    Task UpdateAsync(AdminUser adminUser, CancellationToken cancellationToken);
    Task<long> CountAsync(CancellationToken cancellationToken);
}
