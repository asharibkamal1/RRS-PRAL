using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PralPer.Domain.Entities;

namespace PralPer.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> b)
    {
        b.Property(p => p.Name).HasMaxLength(150).IsRequired();
        b.Property(p => p.Description).HasMaxLength(400);
        b.HasIndex(p => p.Name).IsUnique();
    }
}
