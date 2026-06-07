using PralPer.Domain.Enums;
using PralPer.Infrastructure.Services;
using Xunit;

namespace PralPer.UnitTests;

public class AdminPeriodServiceTests
{
    [Fact]
    public async Task CreateEvaluationPeriod_ActivatesNew_AndArchivesPrevious()
    {
        using var t = new TestDb();
        var svc = new AdminPeriodService(t.Db);

        await svc.CreateEvaluationPeriodAsync(new DateTime(2025, 1, 1), new DateTime(2025, 12, 31));
        await svc.CreateEvaluationPeriodAsync(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));

        var rows = await svc.GetEvaluationPeriodsAsync();
        Assert.Equal(2, rows.Count);
        Assert.Single(rows, r => r.IsActive);                 // exactly one active
        Assert.True(rows[0].IsActive);                        // newest (ordered desc) is the active one
        Assert.Equal(new DateTime(2026, 1, 1), rows[0].StartDate);
    }

    [Fact]
    public async Task CreateEvaluationPeriod_RejectsEndBeforeStart()
    {
        using var t = new TestDb();
        var svc = new AdminPeriodService(t.Db);

        var res = await svc.CreateEvaluationPeriodAsync(new DateTime(2026, 6, 1), new DateTime(2026, 1, 1));

        Assert.False(res.Succeeded);
        Assert.Empty(await svc.GetEvaluationPeriodsAsync());
    }

    [Fact]
    public async Task ActivateEvaluationPeriod_MovesActiveFlag()
    {
        using var t = new TestDb();
        var svc = new AdminPeriodService(t.Db);
        await svc.CreateEvaluationPeriodAsync(new DateTime(2025, 1, 1), new DateTime(2025, 12, 31));
        await svc.CreateEvaluationPeriodAsync(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));

        var older = (await svc.GetEvaluationPeriodsAsync()).Single(r => r.StartDate.Year == 2025);
        var res = await svc.ActivateEvaluationPeriodAsync(older.Id);

        Assert.True(res.Succeeded);
        var rows = await svc.GetEvaluationPeriodsAsync();
        Assert.Single(rows, r => r.IsActive);
        Assert.True(rows.Single(r => r.Id == older.Id).IsActive);
    }

    [Fact]
    public async Task CreateRatingPeriod_RequiresActiveEvaluationPeriod()
    {
        using var t = new TestDb();
        var svc = new AdminPeriodService(t.Db);

        var res = await svc.CreateRatingPeriodAsync(new DateTime(2026, 2, 1), new DateTime(2026, 2, 10));

        Assert.False(res.Succeeded);
    }

    [Fact]
    public async Task CreateRatingPeriod_AttachesToActiveEvaluationPeriod()
    {
        using var t = new TestDb();
        var period = t.SeedActivePeriod();
        var svc = new AdminPeriodService(t.Db);

        var res = await svc.CreateRatingPeriodAsync(new DateTime(2026, 2, 1), new DateTime(2026, 2, 10));

        Assert.True(res.Succeeded);
        var rows = await svc.GetRatingPeriodsAsync();
        Assert.Single(rows);
        Assert.True(rows[0].IsActive);
    }

    [Fact]
    public async Task CreateGoalWindow_StoresAllowAndRestriction()
    {
        using var t = new TestDb();
        t.SeedActivePeriod();
        var dept = t.SeedDepartment("DB");
        var svc = new AdminPeriodService(t.Db);

        var res = await svc.CreateGoalWindowAsync(new DateTime(2026, 3, 1), new DateTime(2026, 3, 5),
            allowSubmission: false, restrictDepartmentId: dept.Id, restrictEmployeeId: null);

        Assert.True(res.Succeeded);
        var rows = await svc.GetGoalWindowsAsync();
        var row = Assert.Single(rows);
        Assert.False(row.AllowSubmission);
        Assert.Equal("DB", row.RestrictDepartment);
    }
}
