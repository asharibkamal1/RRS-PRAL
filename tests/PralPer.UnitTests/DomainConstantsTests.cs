using PralPer.Domain.Constants;
using Xunit;

namespace PralPer.UnitTests;

public class DomainConstantsTests
{
    [Fact]
    public void RoleNames_DefinesThreeRoles()
    {
        Assert.Equal(3, RoleNames.All.Count);
        Assert.Contains(RoleNames.Admin, RoleNames.All);
        Assert.Contains(RoleNames.Manager, RoleNames.All);
        Assert.Contains(RoleNames.Employee, RoleNames.All);
    }
}
