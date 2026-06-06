using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace PralPer.Infrastructure.Identity;

/// <summary>Adds display name + linked EmployeeId as claims so the UI/services can read them.</summary>
public sealed class AppUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    public AppUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options) { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        if (!string.IsNullOrEmpty(user.DisplayName))
            identity.AddClaim(new Claim("display_name", user.DisplayName));

        if (user.EmployeeId is int empId)
            identity.AddClaim(new Claim("employee_id", empId.ToString()));

        return identity;
    }
}
