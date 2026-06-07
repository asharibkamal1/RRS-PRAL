using PralPer.Infrastructure.Services;
using Xunit;

namespace PralPer.UnitTests;

public class AssignRaterServiceTests
{
    [Fact]
    public async Task SaveAssignments_AddsAndRemovesRaters()
    {
        using var t = new TestDb();
        t.SeedActivePeriod();
        var dept = t.SeedDepartment("IT");
        var ratee = t.SeedEmployee("Ratee", dept.Id);
        var raterA = t.SeedEmployee("Rater A", dept.Id);
        var raterB = t.SeedEmployee("Rater B", dept.Id);
        var svc = new AssignRaterService(t.Db);

        // assign A and B
        var save1 = await svc.SaveAssignmentsAsync(ratee.Id, new[] { raterA.Id, raterB.Id });
        Assert.True(save1.Succeeded);
        var view1 = await svc.GetAssignmentAsync(ratee.Id);
        Assert.NotNull(view1);
        Assert.Equal(2, view1!.Ratee.AssignedCount);
        Assert.True(view1.Candidates.Single(c => c.Id == raterA.Id).IsAssigned);

        // re-save with only A → B removed
        var save2 = await svc.SaveAssignmentsAsync(ratee.Id, new[] { raterA.Id });
        Assert.True(save2.Succeeded);
        var view2 = await svc.GetAssignmentAsync(ratee.Id);
        Assert.Equal(1, view2!.Ratee.AssignedCount);
        Assert.False(view2.Candidates.Single(c => c.Id == raterB.Id).IsAssigned);
    }

    [Fact]
    public async Task SaveAssignments_IgnoresSelfAssignment()
    {
        using var t = new TestDb();
        t.SeedActivePeriod();
        var dept = t.SeedDepartment("IT");
        var ratee = t.SeedEmployee("Ratee", dept.Id);
        var svc = new AssignRaterService(t.Db);

        await svc.SaveAssignmentsAsync(ratee.Id, new[] { ratee.Id });

        var view = await svc.GetAssignmentAsync(ratee.Id);
        Assert.Equal(0, view!.Ratee.AssignedCount);
    }

    [Fact]
    public async Task GetRateeStatus_ReportsHasRaters()
    {
        using var t = new TestDb();
        t.SeedActivePeriod();
        var dept = t.SeedDepartment("IT");
        var ratee = t.SeedEmployee("Ratee", dept.Id);
        var rater = t.SeedEmployee("Rater", dept.Id);
        var svc = new AssignRaterService(t.Db);
        await svc.SaveAssignmentsAsync(ratee.Id, new[] { rater.Id });

        var status = await svc.GetRateeStatusAsync();

        Assert.True(status.Single(r => r.Id == ratee.Id).HasRaters);
        Assert.False(status.Single(r => r.Id == rater.Id).HasRaters);
    }

    [Fact]
    public async Task SaveAssignments_FailsWithoutActivePeriod()
    {
        using var t = new TestDb();
        var dept = t.SeedDepartment("IT");
        var ratee = t.SeedEmployee("Ratee", dept.Id);
        var rater = t.SeedEmployee("Rater", dept.Id);
        var svc = new AssignRaterService(t.Db);

        var res = await svc.SaveAssignmentsAsync(ratee.Id, new[] { rater.Id });

        Assert.False(res.Succeeded);
    }
}
