namespace PralPer.Domain.Constants;

/// <summary>Canonical role names used across Identity, claims and authorization policies.</summary>
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Employee = "Employee";

    public static readonly IReadOnlyList<string> All = new[] { Admin, Manager, Employee };
}
