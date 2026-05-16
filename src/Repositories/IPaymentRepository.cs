using backend.src.DTOs;
using backend.src.Models;

namespace backend.src.Repositories;

public interface IPaymentRepository
{
    Task<(IReadOnlyList<Payment> Items, long Total)> ListAsync(PaymentQuery query, CancellationToken cancellationToken);
    Task<Payment?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task UpdateAsync(Payment payment, CancellationToken cancellationToken);
    Task<IReadOnlyList<Payment>> ListSucceededAsync(CancellationToken cancellationToken);
}
