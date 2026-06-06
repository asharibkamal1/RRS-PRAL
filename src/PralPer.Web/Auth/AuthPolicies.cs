namespace PralPer.Web.Auth;

/// <summary>Authorization policy names + the claim used for the active ("viewing as") role.</summary>
public static class AuthPolicies
{
    public const string AdminArea = "AdminArea";
    public const string ManagerArea = "ManagerArea";
    public const string EmployeeArea = "EmployeeArea";

    /// <summary>Claim type carrying the role the user is currently viewing as.</summary>
    public const string ActiveRoleClaim = "active_role";

    /// <summary>Cookie that persists the selected active role across requests.</summary>
    public const string ActiveRoleCookie = "pral.active_role";
}
