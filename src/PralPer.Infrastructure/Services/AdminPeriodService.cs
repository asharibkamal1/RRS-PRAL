using Microsoft.EntityFrameworkCore;
using PralPer.Application.Admin;
using PralPer.Application.Common;
using PralPer.Application.Services;
using PralPer.Domain.Entities;
using PralPer.Domain.Enums;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Services;

/// <summary>Reads/writes evaluation, rating and goal-submission periods for the admin setup screens.</summary>
public sealed class AdminPeriodService : IAdminPeriodService
{
    private readonly AppDbContext _db;

    public AdminPeriodService(AppDbContext db) => _db = db;

    private Task<int> ActiveEvalPeriodIdAsync(CancellationToken ct) =>
        _db.EvaluationPeriods.Where(p => p.Status == PeriodStatus.Active).Select(p => p.Id).FirstOrDefaultAsync(ct);

    private static Result ValidateRange(DateTime start, DateTime end)
        => end.Date <= start.Date ? Result.Failure("End date must be after the start date.") : Result.Success();

    // ---- Evaluation Period ----

    public async Task<IReadOnlyList<PeriodRowDto>> GetEvaluationPeriodsAsync(CancellationToken ct = default)
        => await _db.EvaluationPeriods.AsNoTracking()
            .OrderByDescending(p => p.StartDate)
            .Select(p => new PeriodRowDto(p.Id, p.StartDate, p.EndDate, p.Status == PeriodStatus.Active))
            .ToListAsync(ct);

    public async Task<Result> CreateEvaluationPeriodAsync(DateTime start, DateTime end, CancellationToken ct = default)
    {
        var valid = ValidateRange(start, end);
        if (!valid.Succeeded) return valid;

        foreach (var p in await _db.EvaluationPeriods.Where(p => p.Status == PeriodStatus.Active).ToListAsync(ct))
            p.Status = PeriodStatus.Archived;

        _db.EvaluationPeriods.Add(new EvaluationPeriod
        {
            Name = $"{start:MMM yyyy} - {end:MMM yyyy}",
            StartDate = start,
            EndDate = end,
            Status = PeriodStatus.Active
        });
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ActivateEvaluationPeriodAsync(int id, CancellationToken ct = default)
    {
        var all = await _db.EvaluationPeriods.ToListAsync(ct);
        if (all.All(p => p.Id != id)) return Result.Failure("Evaluation period not found.");
        foreach (var p in all) p.Status = p.Id == id ? PeriodStatus.Active : PeriodStatus.Archived;
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    // ---- Rating Period ----

    public async Task<IReadOnlyList<PeriodRowDto>> GetRatingPeriodsAsync(CancellationToken ct = default)
        => await _db.RatingPeriods.AsNoTracking()
            .OrderByDescending(p => p.StartDate)
            .Select(p => new PeriodRowDto(p.Id, p.StartDate, p.EndDate, p.Status == PeriodStatus.Active))
            .ToListAsync(ct);

    public async Task<Result> CreateRatingPeriodAsync(DateTime start, DateTime end, CancellationToken ct = default)
    {
        var valid = ValidateRange(start, end);
        if (!valid.Succeeded) return valid;

        var evalId = await ActiveEvalPeriodIdAsync(ct);
        if (evalId == 0) return Result.Failure("No active evaluation period. Create one first.");

        foreach (var p in await _db.RatingPeriods.Where(p => p.Status == PeriodStatus.Active).ToListAsync(ct))
            p.Status = PeriodStatus.Archived;

        _db.RatingPeriods.Add(new RatingPeriod
        {
            EvaluationPeriodId = evalId, StartDate = start, EndDate = end, Status = PeriodStatus.Active
        });
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ActivateRatingPeriodAsync(int id, CancellationToken ct = default)
    {
        var all = await _db.RatingPeriods.ToListAsync(ct);
        if (all.All(p => p.Id != id)) return Result.Failure("Rating period not found.");
        foreach (var p in all) p.Status = p.Id == id ? PeriodStatus.Active : PeriodStatus.Archived;
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    // ---- Goal Submission ----

    public async Task<IReadOnlyList<GoalWindowRowDto>> GetGoalWindowsAsync(CancellationToken ct = default)
        => await _db.GoalSubmissionWindows.AsNoTracking()
            .OrderByDescending(w => w.StartDate)
            .Select(w => new GoalWindowRowDto(
                w.Id, w.StartDate, w.EndDate, w.AllowSubmission,
                w.RestrictDepartmentId == null ? null : w.RestrictDepartment!.Name,
                w.RestrictEmployeeId == null ? null : w.RestrictEmployee!.Name))
            .ToListAsync(ct);

    public async Task<Result> CreateGoalWindowAsync(DateTime start, DateTime end, bool allowSubmission,
        int? restrictDepartmentId, int? restrictEmployeeId, CancellationToken ct = default)
    {
        var valid = ValidateRange(start, end);
        if (!valid.Succeeded) return valid;

        var evalId = await ActiveEvalPeriodIdAsync(ct);
        if (evalId == 0) return Result.Failure("No active evaluation period. Create one first.");

        foreach (var w in await _db.GoalSubmissionWindows.Where(w => w.Status == PeriodStatus.Active).ToListAsync(ct))
            w.Status = PeriodStatus.Archived;

        _db.GoalSubmissionWindows.Add(new GoalSubmissionWindow
        {
            EvaluationPeriodId = evalId,
            StartDate = start,
            EndDate = end,
            AllowSubmission = allowSubmission,
            RestrictDepartmentId = restrictDepartmentId,
            RestrictEmployeeId = restrictEmployeeId,
            Status = PeriodStatus.Active
        });
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ActivateGoalWindowAsync(int id, CancellationToken ct = default)
    {
        var all = await _db.GoalSubmissionWindows.ToListAsync(ct);
        if (all.All(w => w.Id != id)) return Result.Failure("Goal submission window not found.");
        foreach (var w in all) w.Status = w.Id == id ? PeriodStatus.Active : PeriodStatus.Archived;
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    // ---- Dropdowns ----

    public async Task<IReadOnlyList<AdminDeptOption>> GetDepartmentsAsync(CancellationToken ct = default)
        => await _db.Departments.OrderBy(d => d.Name)
            .Select(d => new AdminDeptOption(d.Id, d.Name)).ToListAsync(ct);

    public async Task<IReadOnlyList<AdminEmployeeOption>> SearchEmployeesAsync(string? term, CancellationToken ct = default)
    {
        var q = _db.Employees.AsNoTracking().Where(e => e.IsActive);
        if (!string.IsNullOrWhiteSpace(term))
        {
            var s = term.Trim();
            q = q.Where(e => e.Name.Contains(s));
        }
        return await q.OrderBy(e => e.Name).Take(20)
            .Select(e => new AdminEmployeeOption(e.Id, e.Name)).ToListAsync(ct);
    }
}
