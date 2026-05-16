namespace backend.src.Services;

public sealed record EmailSendResult(bool Sent, string Status, string? Error);

public interface IEmailService
{
    Task<EmailSendResult> SendAsync(string subject, string htmlBody, IReadOnlyList<string> recipients, CancellationToken cancellationToken);
}
