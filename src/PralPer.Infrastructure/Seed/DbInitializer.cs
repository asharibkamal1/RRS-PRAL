using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Seed;

/// <summary>Applies migrations then seeds reference + identity data. Call once at startup.</summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        await DataSeeder.SeedAsync(db);                 // reference + sample data (fresh DB only)
        await CompetencyCatalogSeeder.SeedAsync(db);    // competency catalog + job levels + level maps (idempotent)
        await DataSeeder.SeedProfileHistoryAsync(db);   // profile history for all employees (idempotent)
        await DataSeeder.SeedPerReportAsync(db);        // finalized PER report (idempotent)
        await ManagerDashboardSeeder.SeedAsync(db);     // manager dashboard dummy snapshot (idempotent)
        await ManagerTeamSeeder.SeedAsync(db);          // manager's team + submitted goals (idempotent)
        await AdminDemoDataSeeder.SeedAsync(db);        // broad demo population so the admin dashboard is full (idempotent)
        await AdminPeriodsSeeder.SeedAsync(db);         // archived evaluation/rating/goal periods for the admin setup screens (idempotent)
        await ManagerPerReportsSeeder.SeedAsync(db);    // PER reports for remaining employees (idempotent)
        await PermissionCatalogSeeder.SeedAsync(db);    // default permission catalog (admin-managed afterwards)
        await IdentitySeeder.SeedAsync(sp);             // roles + demo users linked to those employees
        await HrmsUserProvisioner.SeedAsync(sp);        // a login per active HRMS employee (temp pwd + forced reset)
    }
}
