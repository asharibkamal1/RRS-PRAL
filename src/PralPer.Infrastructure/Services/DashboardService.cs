using Microsoft.EntityFrameworkCore;
using PralPer.Application.Dashboards;
using PralPer.Application.Services;
using PralPer.Domain.Enums;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Services;

/// <summary>
/// Reads pre-calculated dashboard data from the database. No score computation happens here —
/// scores/percentages come straight from DB tables (seeded as dummies now; produced by the
/// Database team's stored procedures in production).
/// </summary>
public sealed class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db) => _db = db;

    private Task<int> ActivePeriodIdAsync(CancellationToken ct) =>
        _db.EvaluationPeriods
            .Where(p => p.Status == PeriodStatus.Active)
            .Select(p => p.Id)
            .FirstOrDefaultAsync(ct);

    public async Task<EmployeeDashboardDto?> GetEmployeeDashboardAsync(int employeeId, CancellationToken ct = default)
    {
        var periodId = await ActivePeriodIdAsync(ct);
        if (periodId == 0) return null;

        var summary = await _db.EmployeeEvaluationSummaries
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.EvaluationPeriodId == periodId && s.EmployeeId == employeeId, ct);

        var goals = await _db.Goals
            .AsNoTracking()
            .Where(g => g.EvaluationPeriodId == periodId && g.EmployeeId == employeeId)
            .OrderBy(g => g.GoalNo)
            .Select(g => new GoalProgressDto(g.Title, g.WeightPercent, g.ProgressPercent))
            .ToListAsync(ct);

        // Peer evaluations this employee owes (as a rator), split by completion.
        var assignments = await _db.RatorAssignments
            .AsNoTracking()
            .Where(a => a.EvaluationPeriodId == periodId && a.RatorEmployeeId == employeeId)
            .Select(a => new
            {
                a.RateeEmployeeId,
                Name = a.RateeEmployee!.Name,
                Designation = a.RateeEmployee!.Designation!.Name,
                Department = a.RateeEmployee!.Department!.Name
            })
            .ToListAsync(ct);

        var doneRateeIds = await _db.CompetencyRatings
            .AsNoTracking()
            .Where(r => r.RatorEmployeeId == employeeId && r.Status == RatingStatus.Done)
            .Select(r => r.RateeEmployeeId)
            .Distinct()
            .ToListAsync(ct);

        var daysLeft = summary?.DaysUntilDeadline;
        var pending = assignments.Where(a => !doneRateeIds.Contains(a.RateeEmployeeId))
            .Select(a => new PeerEvaluationDto(a.Name, a.Designation, a.Department, "Pending", DaysLeft: daysLeft)).ToList();
        var completed = assignments.Where(a => doneRateeIds.Contains(a.RateeEmployeeId))
            .Select(a => new PeerEvaluationDto(a.Name, a.Designation, a.Department, "Submitted",
                SubmittedOn: summary?.FeedbackUpdatedOn?.ToString("MMM d"))).ToList();

        return new EmployeeDashboardDto(
            summary?.GoalCompletionPercent ?? 0,
            summary?.EvaluationStatus ?? "Not Started",
            summary?.FinalPerScore ?? 0,
            summary?.PendingActions ?? 0,
            summary?.PendingEvaluations ?? pending.Count,
            summary?.SubmittedEvaluations ?? completed.Count,
            summary?.DaysUntilDeadline ?? 0,
            summary?.EvaluationDeadline,
            summary?.ManagerStrengths,
            summary?.ManagerDevelopmentAreas,
            summary?.ManagerName,
            goals, pending, completed);
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct = default)
    {
        var periodId = await ActivePeriodIdAsync(ct);
        var cycle = await _db.EvaluationPeriods.Where(p => p.Id == periodId).Select(p => p.Name).FirstOrDefaultAsync(ct) ?? "—";

        var total = await _db.Employees.CountAsync(e => e.IsActive, ct);
        var completed = await _db.EmployeeEvaluationSummaries
            .CountAsync(s => s.EvaluationPeriodId == periodId &&
                             (s.EvaluationStatus == "Completed" || s.EvaluationStatus == "Approved"), ct);
        var pending = await _db.EmployeeEvaluationSummaries
            .CountAsync(s => s.EvaluationPeriodId == periodId &&
                             (s.EvaluationStatus == "In Review" || s.EvaluationStatus == "Pending"), ct);
        var withSummary = await _db.EmployeeEvaluationSummaries.CountAsync(s => s.EvaluationPeriodId == periodId, ct);
        var notStarted = Math.Max(0, total - withSummary);

        return new AdminDashboardDto(total, completed, pending, notStarted, cycle);
    }

    public async Task<ManagerDashboardDto> GetManagerDashboardAsync(int managerEmployeeId, CancellationToken ct = default)
    {
        var periodId = await ActivePeriodIdAsync(ct);
        var cycle = await _db.EvaluationPeriods.Where(p => p.Id == periodId).Select(p => p.Name).FirstOrDefaultAsync(ct) ?? "—";

        var teamIds = await _db.Employees
            .Where(e => e.ReportingManagerId == managerEmployeeId && e.IsActive)
            .Select(e => e.Id)
            .ToListAsync(ct);

        var goalsToReview = await _db.Goals
            .CountAsync(g => g.EvaluationPeriodId == periodId && teamIds.Contains(g.EmployeeId), ct);

        var pendingRatings = await _db.RatorAssignments
            .CountAsync(a => a.EvaluationPeriodId == periodId && teamIds.Contains(a.RateeEmployeeId), ct);

        var approvedPers = await _db.EmployeeEvaluationSummaries
            .CountAsync(s => s.EvaluationPeriodId == periodId && teamIds.Contains(s.EmployeeId) &&
                             (s.EvaluationStatus == "Completed" || s.EvaluationStatus == "Approved"), ct);

        var team = await _db.Employees
            .Where(e => teamIds.Contains(e.Id))
            .Select(e => new PeerEvaluationDto(
                e.Name,
                e.Designation!.Name,
                e.Department!.Name,
                _db.EmployeeEvaluationSummaries
                    .Where(s => s.EvaluationPeriodId == periodId && s.EmployeeId == e.Id)
                    .Select(s => s.EvaluationStatus)
                    .FirstOrDefault() ?? "Not Started"))
            .ToListAsync(ct);

        return new ManagerDashboardDto(cycle, teamIds.Count, goalsToReview, pendingRatings, approvedPers, team);
    }
}
