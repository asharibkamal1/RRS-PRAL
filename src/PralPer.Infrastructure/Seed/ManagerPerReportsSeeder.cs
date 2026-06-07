using Microsoft.EntityFrameworkCore;
using PralPer.Domain.Entities;
using PralPer.Domain.Enums;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Seed;

/// <summary>
/// Seeds finalized PER reports for the remaining employees (the demo employee already has one),
/// so the manager's PER Reports list has data. Idempotent — only runs while at most one report
/// exists for the active period.
/// </summary>
public static class ManagerPerReportsSeeder
{
    private static readonly (string Title, decimal Weight, decimal Progress, int Rating)[] GoalTemplate =
    {
        ("Deliver assigned module milestones", 40, 80, 3),
        ("Improve code quality and reviews",   30, 70, 3),
        ("Collaboration and knowledge sharing", 30, 90, 4),
    };

    private static readonly string[] Competencies =
        { "Teamwork & Collaboration", "Takes Ownership", "Problem solving & Innovation", "Leadership", "Integrity" };

    public static async Task SeedAsync(AppDbContext db)
    {
        var periodId = await db.EvaluationPeriods
            .Where(p => p.Status == PeriodStatus.Active).Select(p => p.Id).FirstOrDefaultAsync();
        if (periodId == 0) return;

        // Only seed the extra reports once.
        if (await db.PerReports.CountAsync(r => r.EvaluationPeriodId == periodId) > 1) return;

        var withReport = await db.PerReports
            .Where(r => r.EvaluationPeriodId == periodId)
            .Select(r => r.EmployeeId).ToListAsync();

        var employees = await db.Employees
            .Where(e => e.IsActive && !withReport.Contains(e.Id))
            .OrderBy(e => e.Id)
            .ToListAsync();

        var i = 0;
        foreach (var e in employees)
        {
            // Spread scores across the bands for a realistic list.
            var final = 92m - (i % 5) * 9m;     // 92, 83, 74, 65, 56, 92, ...
            var (band, approved) = Band(final);
            var goalScore = Math.Min(100m, final + 4m);
            var compScore = Math.Max(0m, final - 6m);
            i++;

            db.PerReports.Add(new PerReport
            {
                EvaluationPeriodId = periodId,
                EmployeeId = e.Id,
                GoalScorePercent = goalScore,
                CompetencyScorePercent = compScore,
                GoalWeightPercent = 70m,
                CompetencyWeightPercent = 30m,
                FinalPercent = final,
                Band = band,
                Approved = approved,
                ManagerName = "Abdul Hafeez Butt",
                Strengths = "Reliable delivery on assigned tasks\nGood collaboration with the team",
                DevelopmentAreas = "Take more ownership of cross-team initiatives\nStrengthen documentation habits",
                OverallComments = $"{e.Name} performed consistently during the period with solid contributions to the team.",
                ApprovedBy = approved ? "Abdul Hafeez Butt" : null,
                ApprovedOn = approved ? new DateTime(2026, 5, 13) : null,
                GoalsSubmittedOn = new DateTime(2026, 3, 15),
                Evaluation360On = new DateTime(2026, 4, 10),
                ManagerReviewOn = new DateTime(2026, 4, 25),
                FinalApprovalOn = approved ? new DateTime(2026, 5, 13) : null,
                GoalLines = GoalTemplate.Select((g, idx) => new PerReportGoalLine
                {
                    SortOrder = idx + 1,
                    Title = g.Title,
                    WeightPercent = g.Weight,
                    ProgressPercent = g.Progress,
                    Rating = g.Rating,
                    ContributionPercent = Math.Round(g.Weight * g.Rating / 4m, 1)
                }).ToList(),
                CompetencyLines = Competencies.Select((c, idx) => new PerReportCompetencyLine
                {
                    SortOrder = idx + 1,
                    Competency = c,
                    Score = 2 + ((idx + i) % 3 == 0 ? 1 : 0)
                }).ToList()
            });
        }

        await db.SaveChangesAsync();
    }

    private static (string Band, bool Approved) Band(decimal final) => final switch
    {
        >= 80 => ("Excellent", true),
        >= 70 => ("Very Good", true),
        >= 60 => ("Good", true),
        _     => ("Needs Improvement", false)
    };
}
