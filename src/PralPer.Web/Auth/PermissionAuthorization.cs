using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using PralPer.Application.Identity;
using PralPer.Domain.Constants;

namespace PralPer.Web.Auth;

/// <summary>Requirement: the user must hold a given permission (via a role claim or a user claim).</summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission) => Permission = permission;
}

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // Admins implicitly have every permission.
        if (context.User.IsInRole(RoleNames.Admin) ||
            context.User.Claims.Any(c => c.Type == AppClaims.Permission && c.Value == requirement.Permission))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// Resolves policies named "perm:{permission}" on the fly into a <see cref="PermissionRequirement"/>,
/// so permissions created by an admin at runtime are enforceable without registering policies in code.
/// Use as: <c>[Authorize(Policy = PermissionPolicy.For("Goals.Approve"))]</c>.
/// </summary>
public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    public const string Prefix = "perm:";
    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) => _fallback = new DefaultAuthorizationPolicyProvider(options);

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(policyName[Prefix.Length..]))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }
        return _fallback.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();
}

/// <summary>Helper to build the policy name for a permission.</summary>
public static class PermissionPolicy
{
    public static string For(string permission) => PermissionPolicyProvider.Prefix + permission;
}
