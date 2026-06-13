using PralPer.Application.Common;
using PralPer.Application.Identity;

namespace PralPer.Application.Services;

/// <summary>
/// Admin-only management of Identity: users, roles, the permission catalog, and their
/// assignments. Wraps ASP.NET Core Identity (UserManager/RoleManager) behind the Application layer.
/// </summary>
public interface IIdentityAdminService
{
    // ---- Users ----
    Task<IReadOnlyList<UserListItem>> GetUsersAsync(CancellationToken ct = default);
    Task<UserEditModel?> GetUserAsync(string id, CancellationToken ct = default);
    Task<Result> CreateUserAsync(UserEditModel model, CancellationToken ct = default);
    Task<Result> UpdateUserAsync(UserEditModel model, CancellationToken ct = default);
    Task<Result> SetActiveAsync(string userId, bool active, CancellationToken ct = default);
    /// <summary>Reset to the shared default password and force a fresh first-login (OTP) flow.</summary>
    Task<Result> ResetPasswordAsync(string userId, string? newPassword = null, CancellationToken ct = default);
    Task<Result> DeleteUserAsync(string userId, CancellationToken ct = default);

    // ---- Roles ----
    Task<IReadOnlyList<RoleListItem>> GetRolesAsync(CancellationToken ct = default);
    Task<RoleEditModel?> GetRoleAsync(string id, CancellationToken ct = default);
    Task<Result> CreateRoleAsync(RoleEditModel model, CancellationToken ct = default);
    Task<Result> UpdateRoleAsync(RoleEditModel model, CancellationToken ct = default);
    Task<Result> DeleteRoleAsync(string roleId, CancellationToken ct = default);

    // ---- Permission catalog ----
    Task<IReadOnlyList<PermissionItem>> GetPermissionsAsync(CancellationToken ct = default);
    Task<Result> CreatePermissionAsync(PermissionItem model, CancellationToken ct = default);
    Task<Result> UpdatePermissionAsync(PermissionItem model, CancellationToken ct = default);
    Task<Result> DeletePermissionAsync(int id, CancellationToken ct = default);

    // ---- Bulk ----
    Task<IReadOnlyList<ProvisionableEmployee>> GetProvisionableEmployeesAsync(CancellationToken ct = default);
    Task<BulkResult> BulkCreateAsync(IReadOnlyList<BulkUserRow> rows, string? defaultPassword, CancellationToken ct = default);

    /// <summary>Role names available for assignment (optionally excluding Admin for bulk screens).</summary>
    Task<IReadOnlyList<string>> GetRoleNamesAsync(bool excludeAdmin = false, CancellationToken ct = default);
    string DefaultPassword { get; }
}
