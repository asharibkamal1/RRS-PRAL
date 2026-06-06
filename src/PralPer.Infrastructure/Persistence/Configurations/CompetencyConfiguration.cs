using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PralPer.Domain.Entities;

namespace PralPer.Infrastructure.Persistence.Configurations;

public class CompetencyConfiguration : IEntityTypeConfiguration<Competency>
{
    public void Configure(EntityTypeBuilder<Competency> b)
    {
        b.Property(c => c.Name).HasMaxLength(150).IsRequired();
        b.HasIndex(c => c.Name).IsUnique();
    }
}

public class AttributeItemConfiguration : IEntityTypeConfiguration<AttributeItem>
{
    public void Configure(EntityTypeBuilder<AttributeItem> b)
    {
        b.Property(a => a.Name).HasMaxLength(200).IsRequired();
        b.Property(a => a.Weight).HasPrecision(5, 4);

        b.HasOne(a => a.Competency)
            .WithMany(c => c.Attributes)
            .HasForeignKey(a => a.CompetencyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DesignationConfiguration : IEntityTypeConfiguration<Designation>
{
    public void Configure(EntityTypeBuilder<Designation> b)
    {
        b.Property(d => d.Name).HasMaxLength(150).IsRequired();
        b.HasIndex(d => d.Name).IsUnique();
    }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> b)
    {
        b.Property(d => d.Name).HasMaxLength(150).IsRequired();
        b.HasIndex(d => d.Name).IsUnique();
    }
}

public class DesignationAttributeMapConfiguration : IEntityTypeConfiguration<DesignationAttributeMap>
{
    public void Configure(EntityTypeBuilder<DesignationAttributeMap> b)
    {
        b.Property(m => m.Weight).HasPrecision(5, 4);

        b.HasOne(m => m.Designation)
            .WithMany(d => d.AttributeMaps)
            .HasForeignKey(m => m.DesignationId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(m => m.Attribute)
            .WithMany(a => a.DesignationMaps)
            .HasForeignKey(m => m.AttributeId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(m => new { m.DesignationId, m.AttributeId }).IsUnique();
    }
}
