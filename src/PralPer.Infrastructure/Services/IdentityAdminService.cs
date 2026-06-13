using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PralPer.Application.Common;
using PralPer.Application.Identity;
using PralPer.Application.Services;
using PralPer.Domain.Constants;
using PralPer.Domain.Entities;
using PralPer.Infrastructure.Identity;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Services;

/// <summary>Admin-only Identity management (users, roles, permission catalog) over ASP.NET Identity.</summary>
public sealed class IdentityAdminService : IIdentityAdminService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<ApplicationRole> _roles;
    private readonly AppDbContext _db;

    public string DefaultPassword { get; }

    public IdentityAdminService(
        UserManager<ApplicationUser> users,
        RoleManager<ApplicationRole> roles,
        AppDbContext db,
        IConfiguration config)
    {
        _users = users;
        _roles = roles;
        _db = db;
        DefaultPassword = config["HrmsProvisioning:DefaultPassword"]?.Trim() is { Length: > 0 } dp ? dp : "Pral@12345";
    }

    private static bool IsActive(ApplicationUser u) =>
        !(u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow);

    // ---------------------------------------------------------------- Users
    public async Task<IReadOnlyList<UserListItem>> GetUsersAsync(CancellationToken ct = default)
    {
        var users = await _users.Users.AsNoTracking().ToListAsync(ct);
        var roleNames = await _roles.Roles.AsNoTracking().ToDictionaryAsync(r => r.Id, r => r.Name ?? "", ct);
        var userRoles = await _db.Set<IdentityUserRole<string>>().AsNoTracking().ToListAsync(ct);
        var byUser = userRoles.GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => roleNames.GetValueOrDefault(x.RoleId, "")).Where(n => n.Length > 0).ToList());

        return users.Select(u => new UserListItem
        {
            Id = u.Id,
            Email = u.Email ?? u.UserName ?? "",
            DisplayName = u.DisplayName,
            EmployeeId = u.EmployeeId,
            IsActive = IsActive(u),
            MustChangePassword = u.MustChangePassword,
            Roles = byUser.GetValueOrDefault(u.Id, new List<string>())
        }).OrderBy(u => u.Email).ToList();
    }

    public async Task<UserEditModel?> GetUserAsync(string id, CancellationToken ct = default)
    {
        var u = await _users.FindByIdAsync(id);
        if (u is null) return null;
        var roles = await _users.GetRolesAsync(u);
        var perms = (await _users.GetClaimsAsync(u)).Where(c => c.Type == AppClaims.Permission).Select(c => c.Value).ToList();
        return new UserEditModel
        {
            Id = u.Id,
            Email = u.Email ?? "",
            DisplayName = u.DisplayName,
            EmployeeId = u.EmployeeId,
            Roles = roles.ToList(),
            Permissions = perms,
            IsActive = IsActive(u)
        };
    }

    public async Task<Result> CreateUserAsync(UserEditModel model, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(model.Email)) return Result.Failure("Email is required.");
        if (await _users.FindByEmailAsync(model.Email) is not null) return Result.Failure("A user with that email already exists.");

        var explicitPwd = !string.IsNullOrWhiteSpace(model.Password);
        var user = new ApplicationUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            EmailConfirmed = true,
            DisplayName = string.IsNullOrWhiteSpace(model.DisplayName) ? model.Email.Trim() : model.DisplayName.Trim(),
            EmployeeId = model.EmployeeId,
            MustChangePassword = !explicitPwd,            // default password -> force first-login reset
            LockoutEnabled = true
        };

        var created = await _users.CreateAsync(user, explicitPwd ? model.Password! : DefaultPassword);
        if (!created.Succeeded) return Result.Failure(Errors(created));

        await SyncRolesAsync(user, model.Roles);
        await SyncUserPermissionsAsync(user, model.Permissions);
        await ApplyActiveAsync(user, model.IsActive);
        return Result.Success();
    }

    public async Task<Result> UpdateUserAsync(UserEditModel model, CancellationToken ct = default)
    {
        if (model.Id is null) return Result.Failure("Missing user id.");
        var user = await _users.FindByIdAsync(model.Id);
        if (user is null) return Result.Failure("User not found.");

        user.DisplayName = string.IsNullOrWhiteSpace(model.DisplayName) ? user.DisplayName : model.DisplayName.Trim();
        user.EmployeeId = model.EmployeeId;
        if (!string.IsNullOrWhiteSpace(model.Email) && !string.Equals(model.Email, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            await _users.SetEmailAsync(user, model.Email.Trim());
            await _users.SetUserNameAsync(user, model.Email.Trim());
        }
        var updated = await _users.UpdateAsync(user);
        if (!updated.Succeeded) return Result.Failure(Errors(updated));

        await SyncRolesAsync(user, model.Roles);
        await SyncUserPermissionsAsync(user, model.Permissions);
        await ApplyActiveAsync(user, model.IsActive);
        return Result.Success();
    }

    public async Task<Result> SetActiveAsync(string userId, bool active, CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null) return Result.Failure("User not found.");
        await ApplyActiveAsync(user, active);
        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(string userId, string? newPassword = null, CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null) return Result.Failure("User not found.");

        var explicitPwd = !string.IsNullOrWhiteSpace(newPassword);
        var token = await _users.GeneratePasswordResetTokenAsync(user);
        var res = await _users.ResetPasswordAsync(user, token, explicitPwd ? newPassword! : DefaultPassword);
        if (!res.Succeeded) return Result.Failure(Errors(res));

        user.MustChangePassword = !explicitPwd;     // default password -> force first-login reset again
        user.OtpCodeHash = null; user.OtpExpiresAtUtc = null; user.OtpFailedAttempts = 0; user.OtpVerifiedAtUtc = null;
        await _users.UpdateAsync(user);
        return Result.Success();
    }

    public async Task<Result> DeleteUserAsync(string userId, CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null) return Result.Failure("User not found.");
        var res = await _users.DeleteAsync(user);
        return res.Succeeded ? Result.Success() : Result.Failure(Errors(res));
    }

    // ---------------------------------------------------------------- Roles
    public async Task<IReadOnlyList<RoleListItem>> GetRolesAsync(CancellationToken ct = default)
    {
        var roles = await _roles.Roles.AsNoTracking().ToListAsync(ct);
        var userRoles = await _db.Set<IdentityUserRole<string>>().AsNoTracking().ToListAsync(ct);
        var counts = userRoles.GroupBy(x => x.RoleId).ToDictionary(g => g.Key, g => g.Count());

        var list = new List<RoleListItem>();
        foreach (var r in roles)
        {
            var perms = (await _roles.GetClaimsAsync(r)).Where(c => c.Type == AppClaims.Permission).Select(c => c.Value).ToList();
            list.Add(new RoleListItem
            {
                Id = r.Id, Name = r.Name ?? "", Description = r.Description,
                UserCount = counts.GetValueOrDefault(r.Id, 0), Permissions = perms
            });
        }
        return list.OrderBy(r => r.Name).ToList();
    }

    public async Task<RoleEditModel?> GetRoleAsync(string id, CancellationToken ct = default)
    {
        var r = await _roles.FindByIdAsync(id);
        if (r is null) return null;
        var perms = (await _roles.GetClaimsAsync(r)).Where(c => c.Type == AppClaims.Permission).Select(c => c.Value).ToList();
        return new RoleEditModel { Id = r.Id, Name = r.Name ?? "", Description = r.Description, Permissions = perms };
    }

    public async Task<Result> CreateRoleAsync(RoleEditModel model, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(model.Name)) return Result.Failure("Role name is required.");
        if (await _roles.RoleExistsAsync(model.Name)) return Result.Failure("A role with that name already exists.");

        var role = new ApplicationRole(model.Name.Trim()) { Description = model.Description };
        var res = await _roles.CreateAsync(role);
        if (!res.Succeeded) return Result.Failure(Errors(res));
        await SyncRolePermissionsAsync(role, model.Permissions);
        return Result.Success();
    }

    public async Task<Result> UpdateRoleAsync(RoleEditModel model, CancellationToken ct = default)
    {
        if (model.Id is null) return Result.Failure("Missing role id.");
        var role = await _roles.FindByIdAsync(model.Id);
        if (role is null) return Result.Failure("Role not found.");

        role.Description = model.Description;
        if (!string.IsNullOrWhiteSpace(model.Name) && !string.Equals(model.Name, role.Name, StringComparison.OrdinalIgnoreCase))
            await _roles.SetRoleNameAsync(role, model.Name.Trim());
        var res = await _roles.UpdateAsync(role);
        if (!res.Succeeded) return Result.Failure(Errors(res));
        await SyncRolePermissionsAsync(role, model.Permissions);
        return Result.Success();
    }

    public async Task<Result> DeleteRoleAsync(string roleId, CancellationToken ct = default)
    {
        var role = await _roles.FindByIdAsync(roleId);
        if (role is null) return Result.Failure("Role not found.");
        if (string.Equals(role.Name, RoleNames.Admin, StringComparison.OrdinalIgnoreCase))
            return Result.Failure("The Admin role cannot be deleted.");
        var res = await _roles.DeleteAsync(role);
        return res.Succeeded ? Result.Success() : Result.Failure(Errors(res));
    }

    // ---------------------------------------------------------------- Permission catalog
    public async Task<IReadOnlyList<PermissionItem>> GetPermissionsAsync(CancellationToken ct = default) =>
        await _db.Permissions.AsNoTracking().OrderBy(p => p.Name)
            .Select(p => new PermissionItem { Id = p.Id, Name = p.Name, Description = p.Description, IsActive = p.IsActive })
            .ToListAsync(ct);

    public async Task<Result> CreatePermissionAsync(PermissionItem model, CancellationToken ct = default)
    {
        var name = model.Name?.Trim() ?? "";
        if (name.Length == 0) return Result.Failure("Permission name is required.");
        if (await _db.Permissions.AnyAsync(p => p.Name == name, ct)) return Result.Failure("That permission already exists.");
        _db.Permissions.Add(new Permission { Name = name, Description = model.Description, IsActive = model.IsActive });
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> UpdatePermissionAsync(PermissionItem model, CancellationToken ct = default)
    {
        var p = await _db.Permissions.FirstOrDefaultAsync(x => x.Id == model.Id, ct);
        if (p is null) return Result.Failure("Permission not found.");
        p.Name = model.Name.Trim(); p.Description = model.Description; p.IsActive = model.IsActive;
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> DeletePermissionAsync(int id, CancellationToken ct = default)
    {
        var p = await _db.Permissions.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return Result.Failure("Permission not found.");
        _db.Permissions.Remove(p);
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    // ---------------------------------------------------------------- Bulk
    public async Task<IReadOnlyList<ProvisionableEmployee>> GetProvisionableEmployeesAsync(CancellationToken ct = default)
    {
        var linked = await _users.Users.AsNoTracking()
            .Where(u => u.EmployeeId != null).Select(u => u.EmployeeId!.Value).ToListAsync(ct);
        var linkedSet = linked.ToHashSet();

        return await _db.Employees.AsNoTracking().Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .Select(e => new ProvisionableEmployee
            {
                EmployeeId = e.Id, Name = e.Name, Email = e.WorkEmail, HrCode = e.HrCode,
                HasLogin = linkedSet.Contains(e.Id)
            }).ToListAsync(ct);
    }

    public async Task<BulkResult> BulkCreateAsync(IReadOnlyList<BulkUserRow> rows, string? defaultPassword, CancellationToken ct = default)
    {
        var pwd = string.IsNullOrWhiteSpace(defaultPassword) ? DefaultPassword : defaultPassword!.Trim();
        var result = new BulkResult();

        foreach (var row in rows)
        {
            var email = row.Email?.Trim() ?? "";
            if (email.Length == 0) { result.Skipped++; continue; }
            if (string.Equals(row.Role, RoleNames.Admin, StringComparison.OrdinalIgnoreCase))
            { result.Errors.Add($"{email}: Admin cannot be assigned in bulk."); result.Skipped++; continue; }
            if (await _users.FindByEmailAsync(email) is not null) { result.Skipped++; continue; }
            if (!string.IsNullOrWhiteSpace(row.Role) && !await _roles.RoleExistsAsync(row.Role))
            { result.Errors.Add($"{email}: role '{row.Role}' does not exist."); result.Skipped++; continue; }

            var user = new ApplicationUser
            {
                UserName = email, Email = email, EmailConfirmed = true,
                DisplayName = string.IsNullOrWhiteSpace(row.DisplayName) ? email : row.DisplayName.Trim(),
                EmployeeId = row.EmployeeId, MustChangePassword = true, LockoutEnabled = true
            };
            var created = await _users.CreateAsync(user, pwd);
            if (!created.Succeeded) { result.Errors.Add($"{email}: {Errors(created)}"); result.Skipped++; continue; }
            if (!string.IsNullOrWhiteSpace(row.Role)) await _users.AddToRoleAsync(user, row.Role);
            result.Created++;
        }
        return result;
    }

    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(bool excludeAdmin = false, CancellationToken ct = default)
    {
        var names = await _roles.Roles.AsNoTracking().Select(r => r.Name!).Where(n => n != null).ToListAsync(ct);
        if (excludeAdmin) names = names.Where(n => !string.Equals(n, RoleNames.Admin, StringComparison.OrdinalIgnoreCase)).ToList();
        return names.OrderBy(n => n).ToList();
    }

    // ---------------------------------------------------------------- helpers
    private async Task SyncRolesAsync(ApplicationUser user, List<string> desired)
    {
        var current = await _users.GetRolesAsync(user);
        var add = desired.Except(current, StringComparer.OrdinalIgnoreCase).ToList();
        var remove = current.Except(desired, StringComparer.OrdinalIgnoreCase).ToList();
        if (add.Count > 0) await _users.AddToRolesAsync(user, add);
        if (remove.Count > 0) await _users.RemoveFromRolesAsync(user, remove);
    }

    private async Task SyncUserPermissionsAsync(ApplicationUser user, List<string> desired)
    {
        var current = (await _users.GetClaimsAsync(user)).Where(c => c.Type == AppClaims.Permission).ToList();
        foreach (var c in current.Where(c => !desired.Contains(c.Value)))
            await _users.RemoveClaimAsync(user, c);
        foreach (var p in desired.Where(p => current.All(c => c.Value != p)))
            await _users.AddClaimAsync(user, new Claim(AppClaims.Permission, p));
    }

    private async Task SyncRolePermissionsAsync(ApplicationRole role, List<string> desired)
    {
        var current = (await _roles.GetClaimsAsync(role)).Where(c => c.Type == AppClaims.Permission).ToList();
        foreach (var c in current.Where(c => !desired.Contains(c.Value)))
            await _roles.RemoveClaimAsync(role, c);
        foreach (var p in desired.Where(p => current.All(c => c.Value != p)))
            await _roles.AddClaimAsync(role, new Claim(AppClaims.Permission, p));
    }

    private async Task ApplyActiveAsync(ApplicationUser user, bool active)
    {
        user.LockoutEnabled = true;
        user.LockoutEnd = active ? null : DateTimeOffset.MaxValue;
        await _users.UpdateAsync(user);
    }

    private static string Errors(IdentityResult r) => string.Join(" ", r.Errors.Select(e => e.Description));
}
