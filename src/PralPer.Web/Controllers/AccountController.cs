using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        [FromForm] string email,
        [FromForm] string password,
        [FromForm] bool rememberMe = false,
        [FromForm] string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return LoginError("Please enter your email and password.");

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return LoginError("Invalid credentials.");

        var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: true);
        if (!result.Succeeded)
            return LoginError(result.IsLockedOut ? "Account locked. Try again later." : "Invalid credentials.");

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
