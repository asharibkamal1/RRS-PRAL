using Microsoft.EntityFrameworkCore;
using PralPer.Application.Manager;
using PralPer.Application.Services;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Services;

/// <summary>Reads the dummy manager-dashboard snapshot (fetch only — no calculation).</summary>
public sealed class ManagerDashboardService : IManagerDashboardService
{
    private readonly AppDbContext _db;

    public ManagerDashboardService(AppDbContext db) => _db = db;

    private static readonly Dictionary<string, string> CompetencyColors = new()
    {
        ["Leadership"] = "#2563EB",
        ["Takes Ownership"] = "#7C3AED",
        ["Teamwork & Collaboration"] = "#16A34A",
        ["Integrity"] = "#D97706",
        ["Problem Solving & Innovation"] = "#DC2626",
    };

    public async Task<ManagerHomeDto> GetHomeAsync(CancellationToken ct = default)
    {
        var stat = await _db.ManagerDashboardStats.AsNoTracking().FirstOrDefaultAsync(ct);

        var depts = await _db.DeptPerformances.AsNoTracking().OrderBy(d => d.SortOrder)
            .Select(d => new DeptPerfDto(d.Department, d.Completed, d.Pending)).ToListAsync(ct);

        var approvals = await _db.ManagerApprovals.AsNoTracking().OrderBy(a => a.SortOrder)
            .Select(a => new ApprovalDto(a.EmployeeName, a.ApprovalType, a.AgoText)).ToListAsync(ct);

        var raters = await _db.ManagerRaters.AsNoTracking().OrderBy(r => r.SortOrder)
            .Select(r => new RaterRowDto(
                r.Id, r.Name, r.IsActiveRator,
                r.Evaluations.Count,
                r.Evaluations.Count(e => e.Status == "Completed"),
                r.Evaluations.Count(e => e.Status == "Pending")))
            .ToListAsync(ct);

        return new ManagerHomeDto(
            stat?.TeamEvaluationProgress ?? 0,
            stat?.PendingApprovals ?? 0,
            stat?.AverageTeamScore ?? 0,
            stat?.EmployeesAwaitingReview ?? 0,
            stat?.TotalEvaluationsAssigned ?? 0,
            stat?.PendingEvaluations ?? 0,
            stat?.SubmittedEvaluations ?? 0,
            stat?.DaysUntilDeadline ?? 0,
            depts, approvals, raters);
    }

    public async Task<RaterEvaluationsDto?> GetRaterEvaluationsAsync(int raterId, CancellationToken ct = default)
    {
        var rater = await _db.ManagerRaters.AsNoTracking().FirstOrDefaultAsync(r => r.Id == raterId, ct);
        if (rater is null) return null;

        var ratees = await _db.PeerEvaluationSnapshots.AsNoTracking()
            .Where(s => s.RaterId == raterId).OrderBy(s => s.SortOrder)
            .Select(s => new RateeRowDto(s.Id, s.RateeName, s.RateeDepartment, s.Status))
            .ToListAsync(ct);

        var period = await _db.PeerEvaluationSnapshots.Where(s => s.RaterId == raterId)
            .Select(s => s.PeriodName).FirstOrDefaultAsync(ct) ?? "Q1- 2026";

        return new RaterEvaluationsDto(rater.Name, "Senior Software Engineer • IT Department", period, ratees);
    }

    public async Task<PeerEvaluationDetailDto?> GetEvaluationDetailAsync(int snapshotId, CancellationToken ct = default)
    {
        var s = await _db.PeerEvaluationSnapshots.AsNoTracking()
            .Include(x => x.Lines)
            .Include(x => x.Rater)
            .FirstOrDefaultAsync(x => x.Id == snapshotId, ct);
        if (s is null) return null;

        var groups = s.Lines
            .OrderBy(l => l.SortOrder)
            .GroupBy(l => l.Competency)
            .Select(g => new PeerCompetencyGroupDto(
                g.Key,
                CompetencyColors.GetValueOrDefault(g.Key, "#2563EB"),
                g.Select(l => new PeerCompetencyLineDto(l.AttributeName, l.Remarks, l.Score, l.MaxScore)).ToList(),
                g.Any() ? Math.Round((decimal)g.Average(l => l.Score), 1) : 0))
            .ToList();

        return new PeerEvaluationDetailDto(
            s.RateeName, s.RateeJobTitle, s.Rater?.Name ?? "", s.PeriodName,
            s.AverageRating, s.CompetenciesRated, s.AssessmentScorePercent, groups);
    }
}
