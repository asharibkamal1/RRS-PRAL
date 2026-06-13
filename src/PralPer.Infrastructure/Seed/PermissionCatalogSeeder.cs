using Microsoft.EntityFrameworkCore;
using PralPer.Domain.Entities;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Seed;

/// <summary>Seeds a default permission catalog (admins manage it from the UI afterwards). Idempotent.</summary>
public static class PermissionCatalogSeeder
{
    // Module.Action style. Admin can add/remove more from the Permissions screen.
    private static readonly (string Name, string Desc)[] Defaults =
    {
        ("Users.View",      "View users"),
        ("Users.Create",    "Create users"),
        ("Users.Edit",      "Edit users"),
        ("Users.Delete",    "Delete users"),
        ("Users.Deactivate","Activate/deactivate users"),
        ("Roles.View",      "View roles"),
        ("Roles.Create",    "Create roles"),
        ("Roles.Edit",      "Edit roles"),
        ("Roles.Delete",    "Delete roles"),
        ("Permissions.Manage", "Manage the permission catalog"),
        ("Goals.View",      "View goals"),
        ("Goals.Create",    "Create goals"),
        ("Goals.Edit",      "Edit goals"),
        ("Goals.Approve",   "Approve goals"),
        ("Periods.View",    "View evaluation/rating periods"),
        ("Periods.Edit",    "Create/edit periods"),
        ("Raters.Assign",   "Assign raters"),
        ("Reports.View",    "View PER reports"),
        ("Reports.Export",  "Export reports"),
    };

    public static async Task SeedAsync(AppDbContext db)
    {
        var existing = await db.Permissions.Select(p => p.Name).ToListAsync();
        var set = existing.ToHashSet();
        var toAdd = Defaults.Where(d => !set.Contains(d.Name))
            .Select(d => new Permission { Name = d.Name, Description = d.Desc, IsActive = true })
            .ToList();
        if (toAdd.Count == 0) return;
        db.Permissions.AddRange(toAdd);
        await db.SaveChangesAsync();
    }
}
