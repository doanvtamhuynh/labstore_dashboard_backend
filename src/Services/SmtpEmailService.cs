using System.Net;
using System.Net.Mail;
using backend.src.Config;
using Microsoft.Extensions.Options;

namespace backend.src.Services;

public sealed class SmtpEmailService : IEmailService
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<SmtpOptions> options, ILogger<SmtpEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<EmailSendResult> SendAsync(string subject, string htmlBody, IReadOnlyList<string> recipients, CancellationToken cancellationToken)
    {
        var validRecipients = recipients.Where(IsEmail).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (validRecipients.Count == 0)
        {
            return new EmailSendResult(false, "queued-no-recipient", "No valid email recipients were provided");
        }

        if (string.IsNullOrWhiteSpace(_options.Host))
        {
            return new EmailSendResult(false, "queued-smtp-not-configured", "SMTP host is not configured");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        foreach (var recipient in validRecipients)
        {
            message.To.Add(recipient);
        }

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = string.IsNullOrWhiteSpace(_options.Username)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(_options.Username, _options.Password)
        };

        try
        {
            using var registration = cancellationToken.Register(client.SendAsyncCancel);
            await client.SendMailAsync(message, cancellationToken);
            return new EmailSendResult(true, "sent", null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Email delivery failed");
            return new EmailSendResult(false, "queued-send-failed", ex.Message);
        }
    }

    public static IReadOnlyList<string> SplitRecipients(string value)
    {
        return value.Split([';', ',', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static bool IsEmail(string value)
    {
        try
        {
            _ = new MailAddress(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
