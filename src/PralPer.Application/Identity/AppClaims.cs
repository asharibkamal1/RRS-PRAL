namespace PralPer.Application.Identity;

/// <summary>Custom claim types used across Identity and authorization.</summary>
public static class AppClaims
{
    /// <summary>Claim carrying a granted permission (on a role via RoleClaims or a user via UserClaims).</summary>
    public const string Permission = "permission";
}
