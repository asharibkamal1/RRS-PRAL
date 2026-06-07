using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PralPer.Domain.Entities;

namespace PralPer.Infrastructure.Persistence.Configurations;

public class AdminDashboardStatConfiguration : IEntityTypeConfiguration<AdminDashboardStat>
{
    public void Configure(EntityTypeBuilder<AdminDashboardStat> b)
    {
        b.Property(x => x.EmployeesTrend).HasPrecision(6, 2);
        b.Property(x => x.PendingTrend).HasPrecision(6, 2);
        b.Property(x => x.CompletedTrend).HasPrecision(6, 2);
        b.Property(x => x.ActiveCyclePercent).HasPrecision(6, 2);
        b.Property(x => x.StatusCompletedPercent).HasPrecision(6, 2);
        b.Property(x => x.StatusNotStartedPercent).HasPrecision(6, 2);
        b.Property(x => x.StatusInProgressPercent).HasPrecision(6, 2);
        b.Property(x => x.ActiveCycleName).HasMaxLength(100).IsRequired();
    }
}

public class AdminDashboardSeriesPointConfiguration : IEntityTypeConfiguration<AdminDashboardSeriesPoint>
{
    public void Configure(EntityTypeBuilder<AdminDashboardSeriesPoint> b)
    {
        b.Property(x => x.Category).HasMaxLength(40).IsRequired();
        b.Property(x => x.Label).HasMaxLength(100).IsRequired();
        b.Property(x => x.ValueA).HasPrecision(9, 2);
        b.Property(x => x.ValueB).HasPrecision(9, 2);
    }
}

public class AdminActivityConfiguration : IEntityTypeConfiguration<AdminActivity>
{
    public void Configure(EntityTypeBuilder<AdminActivity> b)
    {
        b.Property(x => x.ActorName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(300).IsRequired();
        b.Property(x => x.AgoText).HasMaxLength(50);
    }
}
