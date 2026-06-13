namespace PralPer.Application.Identity;

/// <summary>A login as shown in the admin user list.</summary>
public sealed class UserListItem
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
    public bool IsActive { get; set; }
    public bool MustChangePassword { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
}

/// <summary>Create/edit payload for a single user.</summary>
public sealed class UserEditModel
{
    public string? Id { get; set; }                 // null = create
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();   // direct user permissions
    public bool IsActive { get; set; } = true;
    /// <summary>Optional explicit password (create only). If empty the default password is used.</summary>
    public string? Password { get; set; }
}

public sealed class RoleListItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int UserCount { get; set; }
    public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();
}

public sealed class RoleEditModel
{
    public string? Id { get; set; }                 // null = create
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public sealed class PermissionItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>One row in a bulk user import (from the Employees table or a CSV/Excel file).</summary>
public sealed class BulkUserRow
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
}

/// <summary>An employee (with a work email) available to provision a login for.</summary>
public sealed class ProvisionableEmployee
{
    public int EmployeeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? HrCode { get; set; }
    public bool HasLogin { get; set; }
}

public sealed class BulkResult
{
    public int Created { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = new();
}
