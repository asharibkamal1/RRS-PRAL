namespace PralPer.Application.Abstractions;

/// <summary>Sends transactional emails (OTP codes, notifications). Implemented in Infrastructure.</summary>
public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
}
