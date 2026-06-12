using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PralPer.Domain.Constants;
using PralPer.Domain.Entities;
using PralPer.Infrastructure.Identity;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Seed;

/// <summary>
/// Provisions an application login for every active HRMS employee.
///
/// HRMS supplies the <b>profile</b> (name, code, department, manager) — never a reusable
/// password. So each employee gets an ASP.NET Core Identity login created with a
/// <b>temporary password</b> and <see cref="ApplicationUser.MustChangePassword"/> = true;
/// they set their own password on first sign-in.
///
/// Login id = the employee's work email. Employees without a work email are skipped unless a
/// <c>HrmsProvisioning:FallbackEmailDomain</c> is configured, in which case a deterministic
/// <c>{hrcode}@{domain}</c> address is synthesized. Idempotent: existing logins are left alone
/// (only missing roles are topped up).
/// </summary>
public static class HrmsUserProvisioner
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("HrmsUserProvisioner");

        if (!config.GetValue("HrmsProvisioning:Enabled", true))
        {
            logger.LogInformation("HRMS provisioning disabled (HrmsProvisioning:Enabled=false).");
            return;
        }

        var db = sp.GetRequiredService<AppDbContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<ApplicationRole>>();

        // Ensure the canonical roles exist (provisioning may run before/without IdentitySeeder).
        foreach (var role in RoleNames.All)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole(role) { Description = $"{role} role" });

        var fallbackDomain = config["HrmsProvisioning:FallbackEmailDomain"]?.Trim();
        // First-login password is the SAME shared default for every provisioned employee; they
        // replace it via the OTP + create-password flow on first sign-in.
        var defaultPassword = config["HrmsProvisioning:DefaultPassword"]?.Trim() is { Length: > 0 } dp
            ? dp : "Pral@12345";
        // Re-apply the default password to not-yet-activated accounts on each run (default on).
        var resyncDefaultPassword = config.GetValue("HrmsProvisioning:ResyncDefaultPassword", true);

        var employees = await db.Employees.AsNoTracking()
            .Where(e => e.IsActive)
            .ToListAsync();

        // Ids that are someone's reporting manager -> get the Manager role in addition to Employee.
        var managerEmployeeIds = employees
            .Where(e => e.ReportingManagerId is int)
            .Select(e => e.ReportingManagerId!.Value)
            .ToHashSet();

        int created = 0, skipped = 0, updated = 0;

        foreach (var emp in employees)
        {
            var email = ResolveLoginEmail(emp, fallbackDomain);
            if (email is null)
            {
                skipped++;
                continue; // no work email and no fallback domain -> cannot provision a login
            }

            var roles = new List<string> { RoleNames.Employee };
            if (managerEmployeeIds.Contains(emp.Id))
                roles.Add(RoleNames.Manager);

            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    DisplayName = emp.Name,
                    EmployeeId = emp.Id,
                    MustChangePassword = true
                };

                var result = await userManager.CreateAsync(user, defaultPassword);
                if (!result.Succeeded)
                {
                    logger.LogWarning("Could not provision login for {Email}: {Errors}",
                        email, string.Join("; ", result.Errors.Select(e => e.Description)));
                    skipped++;
                    continue;
                }
                created++;
            }
            else
            {
                // Keep an existing login linked to the employee.
                if (user.EmployeeId != emp.Id)
                {
                    user.EmployeeId = emp.Id;
                    await userManager.UpdateAsync(user);
                    updated++;
                }

                // Re-sync the shared default password for accounts that have NOT yet completed
                // first-login (MustChangePassword still true). This makes a change to the
                // configured DefaultPassword take effect on the next run, instead of being
                // stuck on whatever default was used when the account was first created.
                // Accounts that already set their own password are never touched.
                if (resyncDefaultPassword && user.MustChangePassword)
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    await userManager.ResetPasswordAsync(user, token, defaultPassword);
                }
            }

            foreach (var role in roles)
                if (!await userManager.IsInRoleAsync(user, role))
                    await userManager.AddToRoleAsync(user, role);
        }

        logger.LogInformation(
            "HRMS provisioning complete. Logins created: {Created}, relinked: {Updated}, skipped (no email): {Skipped}.",
            created, updated, skipped);
    }

    private static string? ResolveLoginEmail(Employee emp, string? fallbackDomain)
    {
        if (!string.IsNullOrWhiteSpace(emp.WorkEmail))
            return emp.WorkEmail.Trim();

        if (string.IsNullOrWhiteSpace(fallbackDomain) || string.IsNullOrWhiteSpace(emp.HrCode))
            return null;

        // Deterministic, e.g. "3657@pral.com.pk" (sanitize anything not email-safe).
        var local = new string(emp.HrCode.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '.').ToArray());
        return $"{local}@{fallbackDomain.TrimStart('@')}";
    }
}
