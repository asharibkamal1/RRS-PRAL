using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PralPer.Application.Abstractions;

namespace PralPer.Infrastructure.Services;

/// <summary>
/// Sends email through a company SMTP server (config section <c>Email:Smtp</c>).
/// If no <c>Host</c> is configured it logs the message instead of sending — so the OTP flow
/// is fully testable in development without a mail server. Fill in the SMTP settings (ideally
/// via user-secrets / environment variables) to send real email.
/// </summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        var s = _config.GetSection("Email:Smtp");
        var host = s["Host"];
        var from = s["From"] ?? "no-reply@pral.com.pk";
        var fromName = s["FromName"] ?? "PRAL PER";

        if (string.IsNullOrWhiteSpace(host))
        {
            // No SMTP configured — log so development can still read the code.
            _logger.LogWarning(
                "Email:Smtp:Host not configured — email NOT sent. To: {To} | Subject: {Subject}\n{Body}",
                toEmail, subject, htmlBody);
            return;
        }

        var port = int.TryParse(s["Port"], out var p) ? p : 587;
        var enableSsl = !bool.TryParse(s["EnableSsl"], out var ssl) || ssl; // default true
        var user = s["User"];
        var password = s["Password"];

        using var message = new MailMessage
        {
            From = new MailAddress(from, fromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(host, port) { EnableSsl = enableSsl };
        if (!string.IsNullOrWhiteSpace(user))
            client.Credentials = new NetworkCredential(user, password);

        await client.SendMailAsync(message, ct);
        _logger.LogInformation("OTP email sent to {To}.", toEmail);
    }
}
