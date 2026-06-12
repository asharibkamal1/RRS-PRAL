namespace PralPer.Application.Abstractions;

/// <summary>Result of verifying a one-time password.</summary>
public enum OtpVerifyResult
{
    Success,
    Invalid,     // wrong code
    Expired,     // code expired
    TooManyAttempts,
    NotFound     // no pending OTP / unknown user
}

/// <summary>
/// First-login one-time-password: generate a code, email it to the user, and verify it.
/// Works off the application user id (string) so the Web layer stays free of Identity types.
/// </summary>
public interface IOtpService
{
    /// <summary>Generate a fresh OTP for the user and email it. Returns the masked target address.</summary>
    Task<string?> GenerateAndSendAsync(string userId, CancellationToken ct = default);

    /// <summary>Verify a submitted code; on success marks the user as OTP-verified.</summary>
    Task<OtpVerifyResult> VerifyAsync(string userId, string code, CancellationToken ct = default);
}
