using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PralPer.Domain.Entities;

namespace PralPer.Infrastructure.Persistence.Configurations;

public class PerReportConfiguration : IEntityTypeConfiguration<PerReport>
{
    public void Configure(EntityTypeBuilder<PerReport> b)
    {
        b.Property(p => p.GoalScorePercent).HasPrecision(5, 2);
        b.Property(p => p.CompetencyScorePercent).HasPrecision(5, 2);
        b.Property(p => p.GoalWeightPercent).HasPrecision(5, 2);
        b.Property(p => p.CompetencyWeightPercent).HasPrecision(5, 2);
        b.Property(p => p.FinalPercent).HasPrecision(5, 2);
        b.Property(p => p.Band).HasMaxLength(50);
        b.Property(p => p.ManagerName).HasMaxLength(200);
        b.Property(p => p.Strengths).HasMaxLength(2000);
        b.Property(p => p.DevelopmentAreas).HasMaxLength(2000);
        b.Property(p => p.OverallComments).HasMaxLength(2000);
        b.Property(p => p.ApprovedBy).HasMaxLength(200);

        b.HasOne(p => p.EvaluationPeriod).WithMany().HasForeignKey(p => p.EvaluationPeriodId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(p => p.Employee).WithMany().HasForeignKey(p => p.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(p => new { p.EvaluationPeriodId, p.EmployeeId }).IsUnique();
    }
}

public class PerReportGoalLineConfiguration : IEntityTypeConfiguration<PerReportGoalLine>
{
    public void Configure(EntityTypeBuilder<PerReportGoalLine> b)
    {
        b.Property(x => x.Title).HasMaxLength(250).IsRequired();
        b.Property(x => x.WeightPercent).HasPrecision(5, 2);
        b.Property(x => x.ProgressPercent).HasPrecision(5, 2);
        b.Property(x => x.ContributionPercent).HasPrecision(5, 2);

        b.HasOne(x => x.PerReport).WithMany(p => p.GoalLines).HasForeignKey(x => x.PerReportId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PerReportCompetencyLineConfiguration : IEntityTypeConfiguration<PerReportCompetencyLine>
{
    public void Configure(EntityTypeBuilder<PerReportCompetencyLine> b)
    {
        b.Property(x => x.Competency).HasMaxLength(150).IsRequired();
        b.HasOne(x => x.PerReport).WithMany(p => p.CompetencyLines).HasForeignKey(x => x.PerReportId).OnDelete(DeleteBehavior.Cascade);
    }
}
