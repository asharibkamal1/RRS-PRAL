using Microsoft.EntityFrameworkCore;
using PralPer.Domain.Entities;
using PralPer.Domain.Enums;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Seed;

/// <summary>
/// Seeds archived evaluation / rating periods and goal-submission windows so the admin setup
/// screens show a populated history (the active records remain as seeded elsewhere). Idempotent.
/// </summary>
public static class AdminPeriodsSeeder
{
    private static readonly (DateTime Start, DateTime End)[] EvalArchives =
    {
        (new(2026, 1, 1), new(2026, 3, 31)),
        (new(2024, 7, 1), new(2025, 6, 30)),
        (new(2023, 7, 1), new(2024, 6, 30)),
        (new(2022, 7, 1), new(2023, 6, 30)),
        (new(2021, 7, 1), new(2022, 6, 30)),
    };

    private static readonly (DateTime Start, DateTime End)[] RatingArchives =
    {
        (new(2026, 3, 24), new(2026, 3, 28)),
        (new(2024, 6, 22), new(2025, 6, 26)),
        (new(2023, 6, 24), new(2024, 6, 27)),
        (new(2022, 6, 24), new(2023, 6, 27)),
        (new(2021, 6, 24), new(2022, 6, 27)),
    };

    // Start, End, AllowSubmission, RestrictDeptName
    private static readonly (DateTime Start, DateTime End, bool Allow, string? Dept)[] GoalArchives =
    {
        (new(2026, 6, 25), new(2026, 6, 27), true, null),
        (new(2026, 3, 24), new(2026, 3, 28), false, "DB"),
        (new(2024, 6, 22), new(2025, 6, 26), false, "QA"),
        (new(2023, 6, 24), new(2024, 6, 27), true, null),
        (new(2022, 6, 24), new(2023, 6, 27), true, null),
        (new(2021, 6, 24), new(2022, 6, 27), false, "HR"),
    };

    public static async Task SeedAsync(AppDbContext db)
    {
        var evalId = await db.EvaluationPeriods
            .Where(p => p.Status == PeriodStatus.Active).Select(p => p.Id).FirstOrDefaultAsync();
        if (evalId == 0) return;

        if (await db.EvaluationPeriods.CountAsync() <= 1)
        {
            foreach (var (start, end) in EvalArchives)
                db.EvaluationPeriods.Add(new EvaluationPeriod
                {
                    Name = $"{start:MMM yyyy} - {end:MMM yyyy}", StartDate = start, EndDate = end, Status = PeriodStatus.Archived
                });
        }

        if (await db.RatingPeriods.CountAsync() <= 1)
        {
            foreach (var (start, end) in RatingArchives)
                db.RatingPeriods.Add(new RatingPeriod
                {
                    EvaluationPeriodId = evalId, StartDate = start, EndDate = end, Status = PeriodStatus.Archived
                });
        }

        if (await db.GoalSubmissionWindows.CountAsync() <= 1)
        {
            var deptIds = await db.Departments
                .Where(d => GoalArchives.Select(g => g.Dept).Contains(d.Name))
                .ToDictionaryAsync(d => d.Name, d => d.Id);

            foreach (var (start, end, allow, dept) in GoalArchives)
                db.GoalSubmissionWindows.Add(new GoalSubmissionWindow
                {
                    EvaluationPeriodId = evalId,
                    StartDate = start,
                    EndDate = end,
                    AllowSubmission = allow,
                    RestrictDepartmentId = dept != null && deptIds.TryGetValue(dept, out var id) ? id : null,
                    Status = PeriodStatus.Archived
                });
        }

        await db.SaveChangesAsync();
    }
}
