using backend.src.DTOs;
using backend.src.Models;
using backend.src.Repositories;

namespace backend.src.Services;

public sealed class SupportService : ISupportService
{
    private readonly ICrudRepository<SupportTicket> _tickets;
    private readonly ICrudRepository<FaqItem> _faq;

    public SupportService(ICrudRepository<SupportTicket> tickets, ICrudRepository<FaqItem> faq)
    {
        _tickets = tickets;
        _faq = faq;
    }

    public async Task<IReadOnlyList<TicketResponse>> ListTicketsAsync(CancellationToken cancellationToken)
    {
        return (await _tickets.ListAsync(cancellationToken)).Select(ToResponse).ToList();
    }

    public async Task<TicketResponse> CreateTicketAsync(TicketRequest request, CancellationToken cancellationToken)
    {
        ValidateTicket(request);
        var item = new SupportTicket
        {
            Subject = request.Subject.Trim(),
            CustomerId = request.CustomerId,
            CustomerEmail = request.CustomerEmail.Trim(),
            Priority = request.Priority,
            Messages = [new TicketMessage { Sender = request.CustomerEmail.Trim(), Content = request.Message.Trim() }]
        };
        await _tickets.CreateAsync(item, cancellationToken);
        return ToResponse(item);
    }

    public async Task<TicketResponse> UpdateTicketAsync(string id, TicketRequest request, CancellationToken cancellationToken)
    {
        ValidateTicket(request);
        var item = await RequireAsync(_tickets, id, cancellationToken);
        item.Subject = request.Subject.Trim();
        item.CustomerId = request.CustomerId;
        item.CustomerEmail = request.CustomerEmail.Trim();
        item.Priority = request.Priority;
        item.UpdatedAtUtc = DateTime.UtcNow;
        await _tickets.UpdateAsync(id, item, cancellationToken);
        return ToResponse(item);
    }

    public Task DeleteTicketAsync(string id, CancellationToken cancellationToken) => _tickets.DeleteAsync(id, cancellationToken);

    public async Task<TicketResponse> AssignTicketAsync(string id, TicketAssignRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AssignedTo))
        {
            throw new InvalidOperationException("Assigned admin is required");
        }

        var item = await RequireAsync(_tickets, id, cancellationToken);
        item.AssignedTo = request.AssignedTo.Trim();
        item.UpdatedAtUtc = DateTime.UtcNow;
        await _tickets.UpdateAsync(id, item, cancellationToken);
        return ToResponse(item);
    }

    public async Task<TicketResponse> UpdateTicketStatusAsync(string id, TicketStatusRequest request, CancellationToken cancellationToken)
    {
        var item = await RequireAsync(_tickets, id, cancellationToken);
        item.Status = request.Status;
        item.UpdatedAtUtc = DateTime.UtcNow;
        await _tickets.UpdateAsync(id, item, cancellationToken);
        return ToResponse(item);
    }

    public async Task<IReadOnlyList<FaqResponse>> ListFaqAsync(CancellationToken cancellationToken)
    {
        return (await _faq.ListAsync(cancellationToken)).OrderBy(item => item.SortOrder).Select(ToResponse).ToList();
    }

    public async Task<FaqResponse> CreateFaqAsync(FaqRequest request, CancellationToken cancellationToken)
    {
        ValidateFaq(request);
        var item = new FaqItem { Question = request.Question.Trim(), Answer = request.Answer.Trim(), Category = request.Category.Trim(), IsPublished = request.IsPublished, SortOrder = request.SortOrder };
        await _faq.CreateAsync(item, cancellationToken);
        return ToResponse(item);
    }

    public async Task<FaqResponse> UpdateFaqAsync(string id, FaqRequest request, CancellationToken cancellationToken)
    {
        ValidateFaq(request);
        var item = await RequireAsync(_faq, id, cancellationToken);
        item.Question = request.Question.Trim();
        item.Answer = request.Answer.Trim();
        item.Category = request.Category.Trim();
        item.IsPublished = request.IsPublished;
        item.SortOrder = request.SortOrder;
        item.UpdatedAtUtc = DateTime.UtcNow;
        await _faq.UpdateAsync(id, item, cancellationToken);
        return ToResponse(item);
    }

    public Task DeleteFaqAsync(string id, CancellationToken cancellationToken) => _faq.DeleteAsync(id, cancellationToken);

    private static async Task<T> RequireAsync<T>(ICrudRepository<T> repository, string id, CancellationToken cancellationToken) where T : class
    {
        return await repository.GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Support item not found");
    }

    private static void ValidateTicket(TicketRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.CustomerId) || string.IsNullOrWhiteSpace(request.CustomerEmail) || string.IsNullOrWhiteSpace(request.Message))
        {
            throw new InvalidOperationException("Ticket subject, customer, email, and message are required");
        }
    }

    private static void ValidateFaq(FaqRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question) || string.IsNullOrWhiteSpace(request.Answer) || string.IsNullOrWhiteSpace(request.Category))
        {
            throw new InvalidOperationException("FAQ question, answer, and category are required");
        }
    }

    private static TicketResponse ToResponse(SupportTicket item)
    {
        return new TicketResponse(item.Id!, item.Subject, item.CustomerId, item.CustomerEmail, item.Status, item.Priority, item.AssignedTo, item.Messages.Select(message => new TicketMessageResponse(message.Id, message.Sender, message.Content, message.CreatedAtUtc)).ToList(), item.CreatedAtUtc);
    }

    private static FaqResponse ToResponse(FaqItem item)
    {
        return new FaqResponse(item.Id!, item.Question, item.Answer, item.Category, item.IsPublished, item.SortOrder);
    }
}
