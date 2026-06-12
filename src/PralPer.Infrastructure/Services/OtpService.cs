using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using PralPer.Application.Abstractions;
using PralPer.Infrastructure.Identity;

namespace PralPer.Infrastructure.Services;

/// <summary>
/// First-login OTP: generates a numeric code, stores only its hash + expiry on the user,
/// emails the plaintext, and verifies submissions (single-use, expiring, attempt-limited).
/// </summary>
public sealed class OtpService : IOtpService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _email;
    private readonly IClock _clock;
    private readonly int _length;
    private readonly int _expiryMinutes;
    private readonly int _maxAttempts;

    public OtpService(
        UserManager<ApplicationUser> userManager,
        IEmailSender email,
        IClock clock,
        IConfiguration config)
    {
        _userManager = userManager;
        _email = email;
        _clock = clock;
        _length = config.GetValue("Otp:Length", 6);
        _expiryMinutes = config.GetValue("Otp:ExpiryMinutes", 10);
        _maxAttempts = config.GetValue("Otp:MaxAttempts", 5);
    }

    public async Task<string?> GenerateAndSendAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user?.Email is null)
            return null;

        var code = GenerateNumericCode(_length);
        user.OtpCodeHash = Hash(code);
        user.OtpExpiresAtUtc = _clock.UtcNow.AddMinutes(_expiryMinutes);
        user.OtpFailedAttempts = 0;
        user.OtpVerifiedAtUtc = null;
        await _userManager.UpdateAsync(user);

        var body =
            $"<p>Dear {System.Net.WebUtility.HtmlEncode(user.DisplayName)},</p>" +
            $"<p>Your PRAL PER verification code is:</p>" +
            $"<p style=\"font-size:24px;font-weight:bold;letter-spacing:3px\">{code}</p>" +
            $"<p>This code expires in {_expiryMinutes} minutes. If you did not try to sign in, ignore this email.</p>";

        await _email.SendAsync(user.Email, "Your PRAL PER verification code", body, ct);
        return Mask(user.Email);
    }

    public async Task<OtpVerifyResult> VerifyAsync(string userId, string code, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || user.OtpCodeHash is null || user.OtpExpiresAtUtc is null)
            return OtpVerifyResult.NotFound;

        if (user.OtpFailedAttempts >= _maxAttempts)
            return OtpVerifyResult.TooManyAttempts;

        if (_clock.UtcNow > user.OtpExpiresAtUtc.Value)
            return OtpVerifyResult.Expired;

        if (!FixedEquals(user.OtpCodeHash, Hash(code?.Trim() ?? string.Empty)))
        {
            user.OtpFailedAttempts++;
            await _userManager.UpdateAsync(user);
            return user.OtpFailedAttempts >= _maxAttempts
                ? OtpVerifyResult.TooManyAttempts
                : OtpVerifyResult.Invalid;
        }

        // Success: consume the code and mark verified.
        user.OtpCodeHash = null;
        user.OtpExpiresAtUtc = null;
        user.OtpFailedAttempts = 0;
        user.OtpVerifiedAtUtc = _clock.UtcNow;
        await _userManager.UpdateAsync(user);
        return OtpVerifyResult.Success;
    }

    private static string GenerateNumericCode(int length)
    {
        var max = (int)Math.Pow(10, length);
        var n = RandomNumberGenerator.GetInt32(0, max);
        return n.ToString().PadLeft(length, '0');
    }

    private static string Hash(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static bool FixedEquals(string a, string b)
        => CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));

    private static string Mask(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1) return email;
        return $"{email[0]}***{email.Substring(at - 1)}";
    }
}
