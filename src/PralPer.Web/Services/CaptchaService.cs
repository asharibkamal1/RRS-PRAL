using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;

namespace PralPer.Web.Services;

/// <summary>A rendered CAPTCHA: an inline SVG image (data URI) plus the protected answer token.</summary>
public sealed record CaptchaChallenge(string ImageDataUri, string Token);

/// <summary>
/// Self-contained image CAPTCHA. Renders distorted text as an SVG and carries the answer in an
/// encrypted, expiring token (via ASP.NET Data Protection) so validation is stateless — no
/// external service, API keys or server session required.
/// </summary>
public interface ICaptchaService
{
    bool Enabled { get; }
    CaptchaChallenge Generate();
    bool Validate(string? token, string? userInput);
}

public sealed class CaptchaService : ICaptchaService
{
    // Excludes visually ambiguous characters (0/O, 1/I/L, etc.).
    private const string Charset = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
    private static readonly string[] InkColors =
        { "#1E3A8A", "#2563EB", "#0F766E", "#B91C1C", "#7C3AED", "#0E7490" };

    private readonly IDataProtector _protector;
    private readonly int _length;
    private readonly int _ttlSeconds;

    public bool Enabled { get; }

    public CaptchaService(IDataProtectionProvider dp, IConfiguration config)
    {
        _protector = dp.CreateProtector("PralPer.Captcha.v1");
        Enabled = config.GetValue("Captcha:Enabled", true);
        _length = config.GetValue("Captcha:Length", 5);
        _ttlSeconds = config.GetValue("Captcha:TtlSeconds", 300);
    }

    public CaptchaChallenge Generate()
    {
        var code = RandomCode(_length);
        var svg = RenderSvg(code);
        var dataUri = "data:image/svg+xml;base64," +
            Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));

        var expiry = DateTimeOffset.UtcNow.AddSeconds(_ttlSeconds).ToUnixTimeSeconds();
        var token = _protector.Protect($"{code}|{expiry}");
        return new CaptchaChallenge(dataUri, token);
    }

    public bool Validate(string? token, string? userInput)
    {
        if (!Enabled) return true;
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(userInput))
            return false;

        string payload;
        try { payload = _protector.Unprotect(token); }
        catch { return false; } // tampered / wrong key / corrupt

        var parts = payload.Split('|');
        if (parts.Length != 2 || !long.TryParse(parts[1], out var expiry))
            return false;
        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiry)
            return false; // expired

        return string.Equals(parts[0], userInput.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static string RandomCode(int length)
    {
        var sb = new StringBuilder(length);
        for (var i = 0; i < length; i++)
            sb.Append(Charset[RandomNumberGenerator.GetInt32(Charset.Length)]);
        return sb.ToString();
    }

    private static string RenderSvg(string code)
    {
        const int w = 180, h = 56;
        var rng = new Random();
        var sb = new StringBuilder();

        sb.Append($"<svg xmlns='http://www.w3.org/2000/svg' width='{w}' height='{h}' viewBox='0 0 {w} {h}'>");
        // Background
        sb.Append($"<rect width='{w}' height='{h}' rx='8' fill='#F1F5F9'/>");

        // Noise lines
        for (var i = 0; i < 5; i++)
        {
            int x1 = rng.Next(w), y1 = rng.Next(h), x2 = rng.Next(w), y2 = rng.Next(h);
            var c = InkColors[rng.Next(InkColors.Length)];
            sb.Append($"<line x1='{x1}' y1='{y1}' x2='{x2}' y2='{y2}' stroke='{c}' stroke-width='1' opacity='0.25'/>");
        }
        // Noise dots
        for (var i = 0; i < 24; i++)
            sb.Append($"<circle cx='{rng.Next(w)}' cy='{rng.Next(h)}' r='1' fill='#94A3B8' opacity='0.5'/>");

        // Characters
        var step = (w - 24) / code.Length;
        for (var i = 0; i < code.Length; i++)
        {
            var x = 16 + i * step + rng.Next(-3, 4);
            var y = 38 + rng.Next(-5, 6);
            var angle = rng.Next(-22, 23);
            var size = rng.Next(28, 36);
            var color = InkColors[rng.Next(InkColors.Length)];
            var ch = System.Net.WebUtility.HtmlEncode(code[i].ToString());
            sb.Append(
                $"<text x='{x}' y='{y}' font-family='Segoe UI, Arial, sans-serif' " +
                $"font-size='{size}' font-weight='700' fill='{color}' " +
                $"transform='rotate({angle} {x} {y})'>{ch}</text>");
        }

        sb.Append("</svg>");
        return sb.ToString();
    }
}
