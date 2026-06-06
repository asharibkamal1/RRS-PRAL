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

        await DataSeeder.SeedAsync(db);     // reference + sample data first (Employee Ids 1..n)
        await IdentitySeeder.SeedAsync(sp); // roles + demo users linked to those employees
    }
}
