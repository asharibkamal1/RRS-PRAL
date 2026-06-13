using PralPer.Domain.Common;

namespace PralPer.Domain.Entities;

/// <summary>
/// A catalog of permissions an admin can assign to roles and users (stored as claims of type
/// "permission"). Managed entirely from the admin UI — not hard-coded.
/// </summary>
public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;   // e.g. "Goals.Approve", "Users.Create"
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
