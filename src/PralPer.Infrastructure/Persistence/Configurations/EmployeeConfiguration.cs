using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PralPer.Domain.Entities;

namespace PralPer.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> b)
    {
        b.Property(e => e.HrCode).HasMaxLength(50).IsRequired();
        b.Property(e => e.AccountsCode).HasMaxLength(50);
        b.Property(e => e.Name).HasMaxLength(200).IsRequired();
        b.Property(e => e.AttendancePercent).HasPrecision(5, 2);

        b.HasIndex(e => e.HrCode).IsUnique();

        // Work email is the login id, so it must be unique when present (filtered: many rows may be null).
        b.HasIndex(e => e.WorkEmail).IsUnique().HasFilter("[WorkEmail] IS NOT NULL");

        b.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(e => e.Designation)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DesignationId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(e => e.ReportingManager)
            .WithMany()
            .HasForeignKey(e => e.ReportingManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
