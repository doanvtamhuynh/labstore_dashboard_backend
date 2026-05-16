using backend.src.Models;

namespace backend.src.DTOs;

public sealed record TicketRequest(string Subject, string CustomerId, string CustomerEmail, SupportTicketPriority Priority, string Message);

public sealed record TicketAssignRequest(string AssignedTo);

public sealed record TicketStatusRequest(SupportTicketStatus Status);

public sealed record TicketMessageResponse(string Id, string Sender, string Content, DateTime CreatedAtUtc);

public sealed record TicketResponse(string Id, string Subject, string CustomerId, string CustomerEmail, SupportTicketStatus Status, SupportTicketPriority Priority, string? AssignedTo, IReadOnlyList<TicketMessageResponse> Messages, DateTime CreatedAtUtc);

public sealed record FaqRequest(string Question, string Answer, string Category, bool IsPublished, int SortOrder);

public sealed record FaqResponse(string Id, string Question, string Answer, string Category, bool IsPublished, int SortOrder);
