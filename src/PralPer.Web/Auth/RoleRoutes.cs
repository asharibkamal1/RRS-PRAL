using PralPer.Domain.Constants;

namespace PralPer.Web.Auth;

/// <summary>Maps a role to its landing dashboard route.</summary>
public static class RoleRoutes
{
    public static string DashboardFor(string? role) => role switch
    {
        RoleNames.Admin => "/admin/dashboard",
        RoleNames.Manager => "/manager/dashboard",
        RoleNames.Employee => "/employee/dashboard",
        _ => "/home"   // custom/admin-created roles land on a generic home
    };
}
