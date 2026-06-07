using Microsoft.EntityFrameworkCore;
using PralPer.Application.Common;
using PralPer.Application.Goals;
using PralPer.Application.Services;
using PralPer.Domain.Enums;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Services;

/// <summary>Reads a team member's submitted goals and persists the manager's Weight + Rating assessment.</summary>
public sealed class ManagerGoalService : IManagerGoalService
{
    private readonly AppDbContext _db;

    public ManagerGoalService(AppDbContext db) => _db = db;

    private Task<int> ActivePeriodIdAsync(CancellationToken ct) =>
        _db.EvaluationPeriods.Where(p => p.Status == PeriodStatus.Active).Select(p => p.Id).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<TeamMemberDto>> GetTeamAsync(int managerEmployeeId, CancellationToken ct = default) =>
        await _db.Employees.AsNoTracking()
            .Where(e => e.ReportingManagerId == managerEmployeeId && e.IsActive)
            .OrderBy(e => e.Name)
            .Select(e => new TeamMemberDto(e.Id, e.Name, e.JobTitle))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<GoalView>> GetGoalsAsync(int employeeId, CancellationToken ct = default)
    {
        var periodId = await ActivePeriodIdAsync(ct);
        if (periodId == 0) return Array.Empty<GoalView>();

        return await _db.Goals.AsNoTracking()
            .Where(g => g.EvaluationPeriodId == periodId && g.EmployeeId == employeeId)
            .OrderBy(g => g.GoalNo)
            .Select(g => new GoalView(g.GoalNo, g.Title, g.Description, g.ProgressPercent, g.WeightPercent, g.Rating))
            .ToListAsync(ct);
    }

    public async Task<Result> SaveAssessmentAsync(int employeeId, IReadOnlyList<GoalInput> goals, CancellationToken ct = default)
    {
        var periodId = await ActivePeriodIdAsync(ct);
        if (periodId == 0) return Result.Failure("No active evaluation period.");

        var filled = goals.Where(g => g.HasContent).ToList();
        if (filled.Count == 0) return Result.Failure("This team member has not submitted any goals.");

        if (filled.Any(g => g.WeightPercent <= 0))
            return Result.Failure("Each goal must have a weight greater than 0%.");

        var totalWeight = filled.Sum(g => g.WeightPercent);
        if (totalWeight != 100m)
            return Result.Failure($"Total weight must equal 100% (currently {totalWeight:0}%).");

        var existing = await _db.Goals
            .Where(g => g.EvaluationPeriodId == periodId && g.EmployeeId == employeeId)
            .ToListAsync(ct);

        foreach (var g in existing)
        {
            var input = goals.FirstOrDefault(x => x.GoalNo == g.GoalNo);
            if (input is null) continue;
            // Manager only changes the assessment fields; title/description/progress stay as submitted.
            g.WeightPercent = input.WeightPercent;
            g.Rating = input.Rating;
        }

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
