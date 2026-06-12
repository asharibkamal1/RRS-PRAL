using Microsoft.AspNetCore.Identity;

namespace PralPer.Infrastructure.Identity;

/// <summary>Application login. May be linked to an HRMS Employee and can hold multiple roles.</summary>
public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Optional link to the HRMS Employee master record.</summary>
    public int? EmployeeId { get; set; }

    /// <summary>
    /// True for accounts provisioned from HRMS with a temporary password — the user is
    /// forced to set a new password on first sign-in before reaching the app.
    /// </summary>
    public bool MustChangePassword { get; set; }

    // --- First-login OTP (emailed) ---------------------------------------------------------
    /// <summary>SHA-256 hash of the current one-time code (never store the plaintext).</summary>
    public string? OtpCodeHash { get; set; }
    /// <summary>When the current OTP expires.</summary>
    public DateTimeOffset? OtpExpiresAtUtc { get; set; }
    /// <summary>Failed OTP attempts for the current code (locks the code past the configured max).</summary>
    public int OtpFailedAttempts { get; set; }
    /// <summary>Set when the user has passed OTP; gates the create-password screen. Cleared after reset.</summary>
    public DateTimeOffset? OtpVerifiedAtUtc { get; set; }
}
