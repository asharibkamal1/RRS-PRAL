using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace PralPer.Web.Auth;

/// <summary>
/// Reads the persisted "active role" cookie and, if the user genuinely holds that role,
/// projects it as an <c>active_role</c> claim so policies/nav can react to the
/// role the user is currently "viewing as".
/// </summary>
public sealed class ActiveRoleClaimsTransformation : IClaimsTransformation
{
    private readonly IHttpContextAccessor _http;

    public ActiveRoleClaimsTransformation(IHttpContextAccessor http) => _http = http;

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
            return Task.FromResult(principal);

        if (principal.HasClaim(c => c.Type == AuthPolicies.ActiveRoleClaim))
            return Task.FromResult(principal);

        var activeRole = _http.HttpContext?.Request.Cookies[AuthPolicies.ActiveRoleCookie];
        if (!string.IsNullOrWhiteSpace(activeRole) && principal.IsInRole(activeRole))
        {
            if (principal.Identity is ClaimsIdentity identity)
                identity.AddClaim(new Claim(AuthPolicies.ActiveRoleClaim, activeRole));
        }

        return Task.FromResult(principal);
    }
}
