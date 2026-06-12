using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PralPer.Application.Abstractions;
using PralPer.Infrastructure.Identity;
using PralPer.Web.Auth;

namespace PralPer.Web.Controllers;

/// <summary>
/// Handles authentication form posts (sign in / out) and the "active role" selection
/// for multi-role accounts. Kept as an MVC controller for reliable cookie sign-in,
/// independent of Blazor render mode.
/// </summary>
[Route("account")]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOtpService _otp;
    private readonly ICaptchaService _captcha;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IOtpService otp,
        ICaptchaService captcha)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _otp = otp;
        _captcha = captcha;
    }

    /// <summary>Returns a fresh CAPTCHA challenge (image + token) so the login page can refresh it.</summary>
    [AllowAnonymous]
    [HttpGet("captcha")]
    public IActionResult Captcha()
    {
        var c = _captcha.Generate();
        return Json(new { image = c.ImageDataUri, token = c.Token });
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        [FromForm] string email,
        [FromForm] string password,
        [FromForm] string? captcha = null,
        [FromForm] string? captchaToken = null,
        [FromForm] bool rememberMe = false,
        [FromForm] string? returnUrl = null)
    {
        // Bot/brute-force gate — checked before credentials so failures don't probe accounts.
        if (!_captcha.Validate(captchaToken, captcha))
            return LoginError("Incorrect security code. Please try again.");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return LoginError("Please enter your email and password.");

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return LoginError("Invalid credentials.");

        var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: true);
        if (!result.Succeeded)
            return LoginError(result.IsLockedOut ? "Account locked. Try again later." : "Invalid credentials.");

        // HRMS-provisioned accounts (still on the shared default password) must verify an
        // emailed OTP, then create their own password, before reaching the app.
        if (user.MustChangePassword)
        {
            await _otp.GenerateAndSendAsync(user.Id);
            return LocalRedirect("/verify-otp");
        }

        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Count == 1)
        {
            SetActiveRoleCookie(roles[0]);
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);
            return LocalRedirect(RoleRoutes.DashboardFor(roles[0]));
        }

        // Multiple roles -> let the user choose ("Continue as")
        return LocalRedirect("/continue");
    }

    [HttpPost("set-active-role")]
    [ValidateAntiForgeryToken]
    public IActionResult SetActiveRole([FromForm] string role)
    {
        if (User.IsInRole(role))
        {
            SetActiveRoleCookie(role);
            return LocalRedirect(RoleRoutes.DashboardFor(role));
        }
        return LocalRedirect("/continue");
    }

    [Authorize]
    [HttpPost("verify-otp")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp([FromForm] string code)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return LocalRedirect("/");

        var result = await _otp.VerifyAsync(user.Id, code ?? string.Empty);
        switch (result)
        {
            case OtpVerifyResult.Success:
                await _signInManager.RefreshSignInAsync(user); // surfaces the otp_verified claim
                return LocalRedirect("/set-password");
            case OtpVerifyResult.Expired:
                return LocalRedirect("/verify-otp?error=" +
                    Uri.EscapeDataString("Code expired. Please request a new one."));
            case OtpVerifyResult.TooManyAttempts:
                return LocalRedirect("/verify-otp?error=" +
                    Uri.EscapeDataString("Too many attempts. Please request a new code."));
            default:
                return LocalRedirect("/verify-otp?error=" + Uri.EscapeDataString("Invalid code."));
        }
    }

    [Authorize]
    [HttpPost("resend-otp")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return LocalRedirect("/");

        await _otp.GenerateAndSendAsync(user.Id);
        return LocalRedirect("/verify-otp?info=" + Uri.EscapeDataString("A new code has been sent to your email."));
    }

    [Authorize]
    [HttpPost("set-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPassword(
        [FromForm] string password,
        [FromForm] string confirmPassword)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return LocalRedirect("/");

        // Must have passed the OTP step first.
        if (user.OtpVerifiedAtUtc is null)
            return LocalRedirect("/verify-otp");

        if (string.IsNullOrWhiteSpace(password) || password != confirmPassword)
            return LocalRedirect("/set-password?error=" + Uri.EscapeDataString("Passwords do not match."));

        // Reset without requiring the old (shared default) password, token-based.
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, password);
        if (!result.Succeeded)
            return LocalRedirect("/set-password?error=" +
                Uri.EscapeDataString(string.Join(" ", result.Errors.Select(e => e.Description))));

        // Clear the first-login markers; from now on it's a normal email+password account.
        user.MustChangePassword = false;
        user.OtpVerifiedAtUtc = null;
        await _userManager.UpdateAsync(user);

        // Per the flow: send them back to the login screen to sign in with the new password.
        await _signInManager.SignOutAsync();
        Response.Cookies.Delete(AuthPolicies.ActiveRoleCookie);
        return LocalRedirect("/?info=" +
            Uri.EscapeDataString("Password created. Please sign in with your new password."));
    }

    // ----- Forgot password (anonymous): email -> OTP -> new password ------------------------

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(
        [FromForm] string email,
        [FromForm] string? captcha = null,
        [FromForm] string? captchaToken = null)
    {
        if (!_captcha.Validate(captchaToken, captcha))
            return LocalRedirect("/forgot-password?error=" +
                Uri.EscapeDataString("Incorrect security code. Please try again."));

        // Send a code only if the account exists — but never reveal which is the case.
        if (!string.IsNullOrWhiteSpace(email))
        {
            var user = await _userManager.FindByEmailAsync(email.Trim());
            if (user is not null)
                await _otp.GenerateAndSendAsync(user.Id);
        }

        return LocalRedirect("/reset-password?email=" + Uri.EscapeDataString(email ?? string.Empty) +
            "&info=" + Uri.EscapeDataString("If that email is registered, we've sent a verification code."));
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(
        [FromForm] string email,
        [FromForm] string code,
        [FromForm] string password,
        [FromForm] string confirmPassword)
    {
        string Back(string msg) => "/reset-password?email=" + Uri.EscapeDataString(email ?? string.Empty) +
            "&error=" + Uri.EscapeDataString(msg);

        if (string.IsNullOrWhiteSpace(email))
            return LocalRedirect("/forgot-password");
        if (string.IsNullOrWhiteSpace(password) || password != confirmPassword)
            return LocalRedirect(Back("Passwords do not match."));

        var user = await _userManager.FindByEmailAsync(email.Trim());
        // Generic failure for both unknown-email and bad-code so we don't leak which emails exist.
        if (user is null || await _otp.VerifyAsync(user.Id, code ?? string.Empty) != OtpVerifyResult.Success)
            return LocalRedirect(Back("Invalid or expired code."));

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, password);
        if (!result.Succeeded)
            return LocalRedirect(Back(string.Join(" ", result.Errors.Select(e => e.Description))));

        // A self-reset also satisfies any pending first-login requirement.
        user.MustChangePassword = false;
        user.OtpVerifiedAtUtc = null;
        await _userManager.UpdateAsync(user);

        return LocalRedirect("/?info=" +
            Uri.EscapeDataString("Password reset. Please sign in with your new password."));
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        Response.Cookies.Delete(AuthPolicies.ActiveRoleCookie);
        return LocalRedirect("/");
    }

    private void SetActiveRoleCookie(string role)
        => Response.Cookies.Append(AuthPolicies.ActiveRoleCookie, role, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(8)
        });

    private IActionResult LoginError(string message)
        => LocalRedirect($"/?error={Uri.EscapeDataString(message)}");
}
