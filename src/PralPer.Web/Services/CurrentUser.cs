using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PralPer.Application.Abstractions;
using PralPer.Web.Auth;

namespace PralPer.Web.Services;

/// <summary>Reads identity/claims from the current HTTP context (server-side rendering).</summary>
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;

    public CurrentUser(IHttpContextAccessor http) => _http = http;

    private ClaimsPrincipal? Principal => _http.HttpContext?.User;

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
