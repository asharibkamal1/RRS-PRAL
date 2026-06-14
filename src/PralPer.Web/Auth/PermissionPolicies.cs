namespace PralPer.Web.Auth;

/// <summary>
/// Permission-based policy names (the "perm:" prefix is resolved by <see cref="PermissionPolicyProvider"/>).
/// Constant strings so they can be used in <c>[Authorize(Policy = ...)]</c> attributes.
/// Values after the prefix must match permission names in the catalog (see PermissionCatalogSeeder).
/// </summary>
public static class PermissionPolicies
{
    public const string UsersView = "perm:Users.View";
    public const string UsersCreate = "perm:Users.Create";
    public const string RolesView = "perm:Roles.View";
    public const string PermissionsManage = "perm:Permissions.Manage";
}
