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
}
