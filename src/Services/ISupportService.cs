using backend.src.DTOs;

namespace backend.src.Services;

public interface ISupportService
{
    Task<IReadOnlyList<TicketResponse>> ListTicketsAsync(CancellationToken cancellationToken);
    Task<TicketResponse> CreateTicketAsync(TicketRequest request, CancellationToken cancellationToken);
    Task<TicketResponse> UpdateTicketAsync(string id, TicketRequest request, CancellationToken cancellationToken);
    Task DeleteTicketAsync(string id, CancellationToken cancellationToken);
    Task<TicketResponse> AssignTicketAsync(string id, TicketAssignRequest request, CancellationToken cancellationToken);
    Task<TicketResponse> UpdateTicketStatusAsync(string id, TicketStatusRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<FaqResponse>> ListFaqAsync(CancellationToken cancellationToken);
    Task<FaqResponse> CreateFaqAsync(FaqRequest request, CancellationToken cancellationToken);
    Task<FaqResponse> UpdateFaqAsync(string id, FaqRequest request, CancellationToken cancellationToken);
    Task DeleteFaqAsync(string id, CancellationToken cancellationToken);
}
