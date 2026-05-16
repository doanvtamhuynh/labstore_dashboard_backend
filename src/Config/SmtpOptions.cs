namespace backend.src.Config;

public sealed class SmtpOptions
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool EnableSsl { get; init; } = true;
    public string FromEmail { get; init; } = "no-reply@labstore.local";
    public string FromName { get; init; } = "Labstore";
    public string DefaultRecipients { get; init; } = string.Empty;
}
