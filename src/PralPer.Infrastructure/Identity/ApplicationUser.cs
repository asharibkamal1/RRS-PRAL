using Microsoft.AspNetCore.Identity;

namespace PralPer.Infrastructure.Identity;

/// <summary>Application login. May be linked to an HRMS Employee and can hold multiple roles.</summary>
public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Optional link to the HRMS Employee master record.</summary>
    public int? EmployeeId { get; set; }
}
