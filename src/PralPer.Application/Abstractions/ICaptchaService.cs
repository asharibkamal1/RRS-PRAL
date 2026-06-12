namespace PralPer.Application.Abstractions;

/// <summary>A rendered CAPTCHA: an inline SVG image (data URI) plus the protected answer token.</summary>
public sealed record CaptchaChallenge(string ImageDataUri, string Token);

/// <summary>
/// Self-contained image CAPTCHA. Renders distorted text as an SVG and carries the answer in an
/// encrypted, expiring token so validation is stateless — no external service, API keys or
/// server session required. Implemented in Infrastructure.
/// </summary>
public interface ICaptchaService
{
    bool Enabled { get; }
    CaptchaChallenge Generate();
    bool Validate(string? token, string? userInput);
}
