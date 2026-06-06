using Microsoft.AspNetCore.Identity;

namespace PralPer.Infrastructure.Identity;

public class ApplicationRole : IdentityRole
{
    public ApplicationRole() { }
    public ApplicationRole(string roleName) : base(roleName) { }

    public string? Description { get; set; }
}
