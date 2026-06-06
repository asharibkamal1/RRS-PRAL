namespace PralPer.Application.Abstractions;

/// <summary>Ambient information about the authenticated user and their active role.</summary>
public interface ICurrentUser
{
    string? UserId { get; }
    string? UserName { get; }
    string? DisplayName { get; }
    int? EmployeeId { get; }

    /// <summary>The role the user is currently "viewing as" (multi-role accounts).</summary>
    string? ActiveRole { get; }

    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    IReadOnlyList<string> Roles { get; }
}
