using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PralPer.Domain.Constants;
using PralPer.Infrastructure.Identity;

namespace PralPer.Infrastructure.Seed;

/// <summary>Seeds the three roles and demo users (one multi-role, to exercise "Continue as").</summary>
public static class IdentitySeeder
{
    public const string DefaultPassword = "Pral@12345";

    public static async Task SeedAsync(IServiceProvider sp)
    {
        var roleManager = sp.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole(role) { Description = $"{role} role" });
        }

        // employee@pral.com.pk -> Employee (linked to seeded Employee Id 1: Aamir Abdul Aziz)
        await EnsureUserAsync(userManager, "employee@pral.com.pk", "Aamir Abdul Aziz", employeeId: 1,
            new[] { RoleNames.Employee });

        // manager@pral.com.pk -> Manager + Employee (multi-role demo)
        await EnsureUserAsync(userManager, "manager@pral.com.pk", "Abdul Hafeez Butt", employeeId: 2,
            new[] { RoleNames.Manager, RoleNames.Employee });

        // admin@pral.com.pk -> Admin + Employee (multi-role demo, matches the "Continue as" screen)
        await EnsureUserAsync(userManager, "admin@pral.com.pk", "Abdul Wadood Sherani", employeeId: 3,
            new[] { RoleNames.Admin, RoleNames.Employee });
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email, string displayName, int? employeeId, string[] roles)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = displayName,
                EmployeeId = employeeId
            };
            await userManager.CreateAsync(user, DefaultPassword);
        }

        foreach (var role in roles)
        {
            if (!await userManager.IsInRoleAsync(user, role))
                await userManager.AddToRoleAsync(user, role);
        }
    }
}
