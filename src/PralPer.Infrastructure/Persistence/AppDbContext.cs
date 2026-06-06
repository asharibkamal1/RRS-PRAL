using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PralPer.Application.Abstractions;
using PralPer.Domain.Common;
using PralPer.Domain.Entities;
using PralPer.Infrastructure.Identity;

namespace PralPer.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
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

    // Configuration
    public DbSet<EvaluationPeriod> EvaluationPeriods => Set<EvaluationPeriod>();
    public DbSet<RatingPeriod> RatingPeriods => Set<RatingPeriod>();
    public DbSet<GoalSubmissionWindow> GoalSubmissionWindows => Set<GoalSubmissionWindow>();
    public DbSet<Competency> Competencies => Set<Competency>();
    public DbSet<AttributeItem> Attributes => Set<AttributeItem>();
    public DbSet<DesignationAttributeMap> DesignationAttributeMaps => Set<DesignationAttributeMap>();

    // Evaluation
    public DbSet<RatorAssignment> RatorAssignments => Set<RatorAssignment>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<CompetencyRating> CompetencyRatings => Set<CompetencyRating>();
    public DbSet<PerResult> PerResults => Set<PerResult>();
    public DbSet<EmployeeEvaluationSummary> EmployeeEvaluationSummaries => Set<EmployeeEvaluationSummary>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
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
