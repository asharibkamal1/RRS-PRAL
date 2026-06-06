using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PralPer.Application.Abstractions;
using PralPer.Application.Services;
using PralPer.Infrastructure.Identity;
using PralPer.Infrastructure.Persistence;
using PralPer.Infrastructure.Repositories;
using PralPer.Infrastructure.Services;

namespace PralPer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        // Repository + Unit of Work
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Stored-procedure execution (DB-team SPs), sharing the EF connection
        services.AddScoped<IStoredProcedureExecutor, StoredProcedureExecutor>();

        // Feature service implementations (data-bound; fetch only — no scoring)
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IGoalService, GoalService>();
        services.AddScoped<ICompetencyService, CompetencyService>();

        // Cross-cutting
        services.AddSingleton<IClock, SystemClock>();

        // ASP.NET Core Identity (cookie-based, app-managed accounts)
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/";
            options.LogoutPath = "/account/logout";
            options.AccessDeniedPath = "/access-denied";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });

        return services;
    }
}
