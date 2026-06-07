using PralPer.Application.Goals;
using PralPer.Domain.Entities;
using PralPer.Infrastructure.Services;
using Xunit;

namespace PralPer.UnitTests;

public class ManagerGoalServiceTests
{
    private static void SeedGoals(TestDb t, int periodId, int employeeId)
    {
        for (var i = 1; i <= 3; i++)
            t.Db.Goals.Add(new Goal
            {
                EvaluationPeriodId = periodId, EmployeeId = employeeId, GoalNo = i,
                Title = $"Goal {i}", ProgressPercent = 50, WeightPercent = 0, Rating = 0
            });
        t.Db.SaveChanges();
    }

    private static List<GoalInput> Inputs(params (int no, decimal weight, int rating)[] rows)
        => rows.Select(r => new GoalInput { GoalNo = r.no, Title = $"Goal {r.no}", WeightPercent = r.weight, Rating = r.rating }).ToList();

    [Fact]
    public async Task SaveAssessment_PersistsWhenWeightsTotal100()
    {
        using var t = new TestDb();
        var period = t.SeedActivePeriod();
        var emp = t.SeedEmployee("Aamir");
        SeedGoals(t, period.Id, emp.Id);
        var svc = new ManagerGoalService(t.Db);

        var res = await svc.SaveAssessmentAsync(emp.Id, Inputs((1, 40, 3), (2, 30, 4), (3, 30, 2)));

        Assert.True(res.Succeeded);
        var goals = await svc.GetGoalsAsync(emp.Id);
        Assert.Equal(40m, goals.Single(g => g.GoalNo == 1).WeightPercent);
        Assert.Equal(4, goals.Single(g => g.GoalNo == 2).Rating);
    }

    [Fact]
    public async Task SaveAssessment_RejectsWhenWeightsNot100()
    {
        using var t = new TestDb();
        var period = t.SeedActivePeriod();
        var emp = t.SeedEmployee("Aamir");
        SeedGoals(t, period.Id, emp.Id);
        var svc = new ManagerGoalService(t.Db);

        var res = await svc.SaveAssessmentAsync(emp.Id, Inputs((1, 40, 3), (2, 30, 4), (3, 20, 2)));

        Assert.False(res.Succeeded);
    }

    [Fact]
    public async Task GetGoals_ReturnsSavedGoalsInOrder()
    {
        using var t = new TestDb();
        var period = t.SeedActivePeriod();
        var emp = t.SeedEmployee("Aamir");
        SeedGoals(t, period.Id, emp.Id);
        var svc = new ManagerGoalService(t.Db);

        var goals = await svc.GetGoalsAsync(emp.Id);

        Assert.Equal(3, goals.Count);
        Assert.Equal(new[] { 1, 2, 3 }, goals.Select(g => g.GoalNo).ToArray());
    }
}
