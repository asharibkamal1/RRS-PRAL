using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace PralPer.Infrastructure.Identity;

/// <summary>Adds display name + linked EmployeeId as claims so the UI/services can read them.</summary>
public sealed class AppUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    /// <summary>Claim type set while a user still owes a forced password reset.</summary>
    public const string MustChangePasswordClaim = "must_change_password";

    /// <summary>Claim type set once the first-login OTP has been passed (gates create-password).</summary>
    public const string OtpVerifiedClaim = "otp_verified";


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

        // Drives the forced first-login password reset (enforced by middleware). The claim
        // disappears once the password is changed and the sign-in is refreshed.
        if (user.MustChangePassword)
            identity.AddClaim(new Claim(MustChangePasswordClaim, "true"));

        // Present only between passing OTP and setting the new password.
        if (user.OtpVerifiedAtUtc is not null)
            identity.AddClaim(new Claim(OtpVerifiedClaim, "true"));

        return identity;
    }
}
