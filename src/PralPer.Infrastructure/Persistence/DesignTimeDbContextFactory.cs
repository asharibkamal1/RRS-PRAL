using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PralPer.Infrastructure.Persistence;

/// <summary>
/// Enables `dotnet ef migrations add ...` from the Infrastructure project without running the host.
/// The Web project's appsettings supplies the real connection string at runtime.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("PRALPER_CONNECTION")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=PralPerDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connection)
            .Options;

        return new AppDbContext(options);
    }
}
