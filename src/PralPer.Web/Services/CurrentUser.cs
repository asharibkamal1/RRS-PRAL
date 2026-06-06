using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using PralPer.Application.Abstractions;
using PralPer.Web.Auth;

namespace PralPer.Web.Services;

/// <summary>
/// Reads identity/claims from the current HTTP context (static SSR / prerender) or, when running
/// inside an interactive Server circuit (no HttpContext), from the AuthenticationStateProvider.
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;
    private readonly AuthenticationStateProvider _authState;

    public CurrentUser(IHttpContextAccessor http, AuthenticationStateProvider authState)
    {
        _http = http;
        _authState = authState;
    }

    private ClaimsPrincipal? Principal
    {
        get
        {
            var fromHttp = _http.HttpContext?.User;
            if (fromHttp?.Identity?.IsAuthenticated == true)
                return fromHttp;

            try
            {
                // In a Server circuit this task is already completed.
                return _authState.GetAuthenticationStateAsync().GetAwaiter().GetResult().User;
            }
            catch
            {
                return fromHttp; // e.g. during startup seeding (no circuit / no context)
            }
        }
    }

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public string? UserId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? UserName => Principal?.Identity?.Name;
    public string? DisplayName => Principal?.FindFirstValue("display_name") ?? UserName;

    public int? EmployeeId
    {
        get
        {
            var raw = Principal?.FindFirstValue("employee_id");
            return int.TryParse(raw, out var id) ? id : null;
        }
    }

    public string? ActiveRole => Principal?.FindFirstValue(AuthPolicies.ActiveRoleClaim);

    public bool IsInRole(string role) => Principal?.IsInRole(role) == true;

    public IReadOnlyList<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();
}
