using Microsoft.EntityFrameworkCore;
using PralPer.Application.Common;
using PralPer.Application.Goals;
using PralPer.Application.Services;
using PralPer.Domain.Entities;
using PralPer.Domain.Enums;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Services;

/// <summary>
/// Persists raw goal rows (Section 2) and reads them back. No PER calculation here —
/// scoring is performed by the database. The weight-total check is a form validation.
/// </summary>
public sealed class GoalService : IGoalService
{
    private readonly AppDbContext _db;

    public GoalService(AppDbContext db) => _db = db;

    private Task<int> ActivePeriodIdAsync(CancellationToken ct) =>
        _db.EvaluationPeriods.Where(p => p.Status == PeriodStatus.Active).Select(p => p.Id).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<GoalView>> GetMyGoalsAsync(int employeeId, CancellationToken ct = default)
    {
        var periodId = await ActivePeriodIdAsync(ct);
        if (periodId == 0) return Array.Empty<GoalView>();

        return await _db.Goals
            .AsNoTracking()
            .Where(g => g.EvaluationPeriodId == periodId && g.EmployeeId == employeeId)
            .OrderBy(g => g.GoalNo)
            .Select(g => new GoalView(g.GoalNo, g.Title, g.Description, g.ProgressPercent, g.WeightPercent, g.Rating))
            .ToListAsync(ct);
    }

    public async Task<bool> IsSubmissionOpenAsync(CancellationToken ct = default)
    {
        var periodId = await ActivePeriodIdAsync(ct);
        if (periodId == 0) return false;

        var today = DateTime.Today;
        return await _db.GoalSubmissionWindows.AnyAsync(w =>
            w.EvaluationPeriodId == periodId &&
            w.AllowSubmission &&
            w.StartDate.Date <= today && today <= w.EndDate.Date, ct);
    }

    public async Task<Result> SaveMyGoalsAsync(int employeeId, IReadOnlyList<GoalInput> goals, CancellationToken ct = default)
    {
        var periodId = await ActivePeriodIdAsync(ct);
        if (periodId == 0) return Result.Failure("No active evaluation period.");

        if (!await IsSubmissionOpenAsync(ct))
            return Result.Failure("Goal submission is currently closed.");

        var filled = goals.Where(g => g.HasContent).ToList();

        if (filled.Count < 3 || filled.Count > 5)
            return Result.Failure("Please submit between 3 and 5 goals.");

        if (filled.Any(g => g.WeightPercent <= 0))
            return Result.Failure("Each goal must have a weight greater than 0%.");

        var totalWeight = filled.Sum(g => g.WeightPercent);
        if (totalWeight != 100m)
            return Result.Failure($"Total weight must equal 100% (currently {totalWeight:0}%).");

        // Replace existing goals for this employee/period with the submitted set.
        var existing = await _db.Goals
            .Where(g => g.EvaluationPeriodId == periodId && g.EmployeeId == employeeId)
            .ToListAsync(ct);
        _db.Goals.RemoveRange(existing);

        var no = 1;
        foreach (var g in filled)
        {
            _db.Goals.Add(new Goal
            {
                EvaluationPeriodId = periodId,
                EmployeeId = employeeId,
                GoalNo = no++,
                Title = g.Title!.Trim(),
                Description = g.Description?.Trim(),
                ProgressPercent = g.ProgressPercent,
                WeightPercent = g.WeightPercent,
                Rating = g.Rating
            });
        }

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
