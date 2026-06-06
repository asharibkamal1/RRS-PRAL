using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PralPer.Domain.Entities;

namespace PralPer.Infrastructure.Persistence.Configurations;

public class PromotionHistoryConfiguration : IEntityTypeConfiguration<PromotionHistory>
{
    public void Configure(EntityTypeBuilder<PromotionHistory> b)
    {
        b.Property(p => p.FromTitle).HasMaxLength(150);
        b.Property(p => p.ToTitle).HasMaxLength(150).IsRequired();
        b.Property(p => p.Note).HasMaxLength(250);

        b.HasOne(p => p.Employee)
            .WithMany(e => e.Promotions)
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class IncrementHistoryConfiguration : IEntityTypeConfiguration<IncrementHistory>
{
    public void Configure(EntityTypeBuilder<IncrementHistory> b)
    {
        b.Property(i => i.Percentage).HasPrecision(5, 2);
        b.Property(i => i.Amount).HasPrecision(12, 2);

        b.HasOne(i => i.Employee)
            .WithMany(e => e.Increments)
            .HasForeignKey(i => i.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class BonusHistoryConfiguration : IEntityTypeConfiguration<BonusHistory>
{
    public void Configure(EntityTypeBuilder<BonusHistory> b)
    {
        b.Property(x => x.BonusType).HasMaxLength(100).IsRequired();
        b.Property(x => x.Quarter).HasMaxLength(30).IsRequired();
        b.Property(x => x.Amount).HasPrecision(12, 2);

        b.HasOne(x => x.Employee)
            .WithMany(e => e.Bonuses)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
