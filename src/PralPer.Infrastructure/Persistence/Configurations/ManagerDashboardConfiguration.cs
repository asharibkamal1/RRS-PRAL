using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PralPer.Domain.Entities;

namespace PralPer.Infrastructure.Persistence.Configurations;

public class ManagerDashboardStatConfiguration : IEntityTypeConfiguration<ManagerDashboardStat>
{
    public void Configure(EntityTypeBuilder<ManagerDashboardStat> b)
    {
        b.Property(x => x.TeamEvaluationProgress).HasPrecision(5, 2);
        b.Property(x => x.AverageTeamScore).HasPrecision(5, 2);
    }
}

public class DeptPerformanceConfiguration : IEntityTypeConfiguration<DeptPerformance>
{
    public void Configure(EntityTypeBuilder<DeptPerformance> b)
        => b.Property(x => x.Department).HasMaxLength(100).IsRequired();
}

public class ManagerApprovalConfiguration : IEntityTypeConfiguration<ManagerApproval>
{
    public void Configure(EntityTypeBuilder<ManagerApproval> b)
    {
        b.Property(x => x.EmployeeName).HasMaxLength(200).IsRequired();
        b.Property(x => x.ApprovalType).HasMaxLength(100).IsRequired();
        b.Property(x => x.AgoText).HasMaxLength(50);
    }
}

public class ManagerRaterConfiguration : IEntityTypeConfiguration<ManagerRater>
{
    public void Configure(EntityTypeBuilder<ManagerRater> b)
        => b.Property(x => x.Name).HasMaxLength(200).IsRequired();
}

public class PeerEvaluationSnapshotConfiguration : IEntityTypeConfiguration<PeerEvaluationSnapshot>
{
    public void Configure(EntityTypeBuilder<PeerEvaluationSnapshot> b)
    {
        b.Property(x => x.RateeName).HasMaxLength(200).IsRequired();
        b.Property(x => x.RateeDepartment).HasMaxLength(100);
        b.Property(x => x.RateeJobTitle).HasMaxLength(150);
        b.Property(x => x.Status).HasMaxLength(30);
        b.Property(x => x.PeriodName).HasMaxLength(100);
        b.Property(x => x.AverageRating).HasPrecision(5, 2);
        b.Property(x => x.AssessmentScorePercent).HasPrecision(5, 2);

        b.HasOne(x => x.Rater).WithMany(r => r.Evaluations).HasForeignKey(x => x.RaterId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PeerEvaluationLineConfiguration : IEntityTypeConfiguration<PeerEvaluationLine>
{
    public void Configure(EntityTypeBuilder<PeerEvaluationLine> b)
    {
        b.Property(x => x.Competency).HasMaxLength(150).IsRequired();
        b.Property(x => x.AttributeName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Remarks).HasMaxLength(400);

        b.HasOne(x => x.Snapshot).WithMany(s => s.Lines).HasForeignKey(x => x.SnapshotId).OnDelete(DeleteBehavior.Cascade);
    }
}
