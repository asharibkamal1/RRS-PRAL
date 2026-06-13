using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PralPer.Application.Abstractions;
using PralPer.Domain.Common;
using PralPer.Domain.Entities;
using PralPer.Infrastructure.Identity;

namespace PralPer.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>, IDataProtectionKeyContext
{
    /// <summary>Shared Data Protection key ring (encrypts the auth cookie) — persisted so all servers share it.</summary>
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    private readonly ICurrentUser? _currentUser;
    private readonly IClock? _clock;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUser? currentUser = null,
        IClock? clock = null) : base(options)
    {
        _currentUser = currentUser;
        _clock = clock;
    }

    // HRMS-sourced (read-only consumption)
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PromotionHistory> PromotionHistories => Set<PromotionHistory>();
    public DbSet<IncrementHistory> IncrementHistories => Set<IncrementHistory>();
    public DbSet<BonusHistory> BonusHistories => Set<BonusHistory>();

    // Configuration
    public DbSet<EvaluationPeriod> EvaluationPeriods => Set<EvaluationPeriod>();
    public DbSet<RatingPeriod> RatingPeriods => Set<RatingPeriod>();
    public DbSet<GoalSubmissionWindow> GoalSubmissionWindows => Set<GoalSubmissionWindow>();
    public DbSet<Competency> Competencies => Set<Competency>();
    public DbSet<AttributeItem> Attributes => Set<AttributeItem>();
    public DbSet<DesignationAttributeMap> DesignationAttributeMaps => Set<DesignationAttributeMap>();
    public DbSet<Permission> Permissions => Set<Permission>();

    // Evaluation
    public DbSet<RatorAssignment> RatorAssignments => Set<RatorAssignment>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<CompetencyRating> CompetencyRatings => Set<CompetencyRating>();
    public DbSet<PerResult> PerResults => Set<PerResult>();
    public DbSet<EmployeeEvaluationSummary> EmployeeEvaluationSummaries => Set<EmployeeEvaluationSummary>();
    public DbSet<PerReport> PerReports => Set<PerReport>();
    public DbSet<PerReportGoalLine> PerReportGoalLines => Set<PerReportGoalLine>();
    public DbSet<PerReportCompetencyLine> PerReportCompetencyLines => Set<PerReportCompetencyLine>();

    // Manager dashboard (dummy snapshot)
    public DbSet<ManagerDashboardStat> ManagerDashboardStats => Set<ManagerDashboardStat>();
    public DbSet<DeptPerformance> DeptPerformances => Set<DeptPerformance>();
    public DbSet<ManagerApproval> ManagerApprovals => Set<ManagerApproval>();
    public DbSet<ManagerRater> ManagerRaters => Set<ManagerRater>();
    public DbSet<PeerEvaluationSnapshot> PeerEvaluationSnapshots => Set<PeerEvaluationSnapshot>();
    public DbSet<PeerEvaluationLine> PeerEvaluationLines => Set<PeerEvaluationLine>();

    // Admin dashboard (dummy snapshot)
    public DbSet<AdminDashboardStat> AdminDashboardStats => Set<AdminDashboardStat>();
    public DbSet<AdminDashboardSeriesPoint> AdminDashboardSeriesPoints => Set<AdminDashboardSeriesPoint>();
    public DbSet<AdminActivity> AdminActivities => Set<AdminActivity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Professional table names for the ASP.NET Identity tables (drop the "AspNet" prefix).
        // Only the table names change — entity classes and all app code are unaffected.
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAudit();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void StampAudit()
    {
        var now = _clock?.UtcNow ?? DateTimeOffset.UtcNow;
        var user = _currentUser?.UserName ?? "system";

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAtUtc = now;
                    entry.Entity.CreatedBy = user;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedAtUtc = now;
                    entry.Entity.ModifiedBy = user;
                    break;
            }
        }
    }
}
