using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PralPer.Domain.Entities;
using PralPer.Infrastructure.Identity;

namespace PralPer.Infrastructure.Persistence.Configurations;

/// <summary>
/// Links an application login to its Employee master record:
/// <c>AspNetUsers.EmployeeId → Employees.Id</c>. Nullable (a login may be unlinked); if the
/// employee row is removed the link is set to NULL rather than cascading the delete to the login.
/// </summary>
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> b)
    {
        b.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(u => u.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
