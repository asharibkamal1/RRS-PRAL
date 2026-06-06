using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PralPer.Domain.Entities;

namespace PralPer.Infrastructure.Persistence.Configurations;

public class EvaluationPeriodConfiguration : IEntityTypeConfiguration<EvaluationPeriod>
{
    public void Configure(EntityTypeBuilder<EvaluationPeriod> b)
    {
        b.Property(p => p.Name).HasMaxLength(100).IsRequired();
        b.Property(p => p.Status).HasConversion<int>();
    }
}

public class RatingPeriodConfiguration : IEntityTypeConfiguration<RatingPeriod>
{
    public void Configure(EntityTypeBuilder<RatingPeriod> b)
    {
        b.Property(p => p.Status).HasConversion<int>();
        b.HasOne(p => p.EvaluationPeriod)
            .WithMany(e => e.RatingPeriods)
            .HasForeignKey(p => p.EvaluationPeriodId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GoalSubmissionWindowConfiguration : IEntityTypeConfiguration<GoalSubmissionWindow>
{
    public void Configure(EntityTypeBuilder<GoalSubmissionWindow> b)
    {
        b.Property(p => p.Status).HasConversion<int>();

        b.HasOne(p => p.EvaluationPeriod)
            .WithMany(e => e.GoalWindows)
            .HasForeignKey(p => p.EvaluationPeriodId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(p => p.RestrictDepartment)
            .WithMany()
            .HasForeignKey(p => p.RestrictDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(p => p.RestrictEmployee)
            .WithMany()
            .HasForeignKey(p => p.RestrictEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RatorAssignmentConfiguration : IEntityTypeConfiguration<RatorAssignment>
{
    public void Configure(EntityTypeBuilder<RatorAssignment> b)
    {
        b.HasOne(r => r.EvaluationPeriod)
            .WithMany()
            .HasForeignKey(r => r.EvaluationPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(r => r.RateeEmployee)
            .WithMany()
            .HasForeignKey(r => r.RateeEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(r => r.RatorEmployee)
            .WithMany()
            .HasForeignKey(r => r.RatorEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(r => new { r.EvaluationPeriodId, r.RateeEmployeeId, r.RatorEmployeeId }).IsUnique();
    }
}

public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> b)
    {
        b.Property(g => g.Title).HasMaxLength(250).IsRequired();
        b.Property(g => g.Description).HasMaxLength(1000);
        b.Property(g => g.ProgressPercent).HasPrecision(5, 2);
        b.Property(g => g.WeightPercent).HasPrecision(5, 2);

        b.HasOne(g => g.EvaluationPeriod)
            .WithMany()
            .HasForeignKey(g => g.EvaluationPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(g => g.Employee)
            .WithMany()
            .HasForeignKey(g => g.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(g => new { g.EvaluationPeriodId, g.EmployeeId, g.GoalNo }).IsUnique();
    }
}

public class CompetencyRatingConfiguration : IEntityTypeConfiguration<CompetencyRating>
{
    public void Configure(EntityTypeBuilder<CompetencyRating> b)
    {
        b.Property(r => r.Remarks).HasMaxLength(1000);
        b.Property(r => r.Status).HasConversion<int>();

        b.HasOne(r => r.RatingPeriod)
            .WithMany()
            .HasForeignKey(r => r.RatingPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(r => r.RateeEmployee)
            .WithMany()
            .HasForeignKey(r => r.RateeEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(r => r.RatorEmployee)
            .WithMany()
            .HasForeignKey(r => r.RatorEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(r => r.Attribute)
            .WithMany()
            .HasForeignKey(r => r.AttributeId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(r => new { r.RatingPeriodId, r.RateeEmployeeId, r.RatorEmployeeId, r.AttributeId }).IsUnique();
    }
}

public class PerResultConfiguration : IEntityTypeConfiguration<PerResult>
{
    public void Configure(EntityTypeBuilder<PerResult> b)
    {
        b.Property(p => p.GoalScore).HasPrecision(5, 2);
        b.Property(p => p.PeerScore).HasPrecision(5, 2);
        b.Property(p => p.FinalScore).HasPrecision(5, 2);

        b.HasOne(p => p.EvaluationPeriod)
            .WithMany()
            .HasForeignKey(p => p.EvaluationPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(p => p.Employee)
            .WithMany()
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(p => new { p.EvaluationPeriodId, p.EmployeeId }).IsUnique();
    }
}
