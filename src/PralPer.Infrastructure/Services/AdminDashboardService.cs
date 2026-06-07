using Microsoft.EntityFrameworkCore;
using PralPer.Application.Dashboards;
using PralPer.Application.Services;
using PralPer.Domain.Enums;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Services;

/// <summary>Builds the Administrator Dashboard entirely from live aggregates over the operational tables.</summary>
public sealed class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _db;

    public AdminDashboardService(AppDbContext db) => _db = db;

    private static readonly (int Score, string Label)[] RatingLabels =
    {
        (0, "Below Expectations"), (1, "Needs Improvement"), (2, "Meets Expectations"),
        (3, "Excellent"), (4, "Outstanding"),
    };

    public async Task<AdminDashboardData> GetAsync(CancellationToken ct = default)
    {
        var period = await _db.EvaluationPeriods
            .Where(p => p.Status == PeriodStatus.Active)
            .Select(p => new { p.Id, p.Name, p.StartDate, p.EndDate })
            .FirstOrDefaultAsync(ct);
        var periodId = period?.Id ?? 0;

        var employees = await _db.Employees.AsNoTracking()
            .Where(e => e.IsActive)
            .Select(e => new { e.Id, Department = e.Department!.Name })
            .ToListAsync(ct);
        var total = employees.Count;

        var reports = await _db.PerReports.AsNoTracking()
            .Where(r => r.EvaluationPeriodId == periodId)
            .Select(r => new { r.EmployeeId, r.FinalPercent, r.Approved, r.ApprovedOn })
            .ToListAsync(ct);
        var approvedSet = reports.Where(r => r.Approved).Select(r => r.EmployeeId).ToHashSet();
        var reportedSet = reports.Select(r => r.EmployeeId).ToHashSet();

        var completed = employees.Count(e => approvedSet.Contains(e.Id));
        var pending = total - completed;

        var kpi = new AdminKpiDto(
            total, null,
            pending, null,
            completed, null,
            period?.Name ?? "—",
            total > 0 ? Math.Round(completed * 100m / total, 0) : 0);

        // ---- Department-wise progress ----
        var deptProgress = employees
            .GroupBy(e => e.Department)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var done = g.Count(e => approvedSet.Contains(e.Id));
                return new AdminDeptProgressDto(g.Key, done, g.Count() - done);
            })
            .ToList();

        // ---- Evaluation timeline (cumulative approvals per month) ----
        var timeline = new List<AdminTimelinePointDto>();
        if (period is not null)
        {
            var approvedDates = reports.Where(r => r.Approved && r.ApprovedOn != null)
                .Select(r => r.ApprovedOn!.Value).ToList();
            var cursor = new DateTime(period.StartDate.Year, period.StartDate.Month, 1);
            for (var i = 0; i < 12 && cursor <= period.EndDate; i++, cursor = cursor.AddMonths(1))
            {
                var monthEnd = cursor.AddMonths(1).AddDays(-1);
                timeline.Add(new AdminTimelinePointDto(cursor.ToString("MMM"), approvedDates.Count(d => d <= monthEnd)));
            }
        }

        // ---- Performance rating distribution (bucket FinalPercent 0-4) ----
        var buckets = new int[5];
        foreach (var r in reports) buckets[Bucket(r.FinalPercent)]++;
        var n = reports.Count;
        var rating = RatingLabels
            .Select(rl => new AdminRatingBucketDto(rl.Score, rl.Label, n > 0 ? Math.Round(buckets[rl.Score] * 100m / n, 0) : 0))
            .ToList();

        // ---- Status distribution ----
        var goalEmpIds = await _db.Goals.AsNoTracking()
            .Where(g => g.EvaluationPeriodId == periodId).Select(g => g.EmployeeId).Distinct().ToListAsync(ct);
        var startedSet = new HashSet<int>(goalEmpIds);
        startedSet.UnionWith(reportedSet);
        var inProgress = employees.Count(e => !approvedSet.Contains(e.Id) && startedSet.Contains(e.Id));
        var notStarted = total - completed - inProgress;
        var status = total > 0
            ? new AdminStatusDto(
                Math.Round(completed * 100m / total, 0),
                Math.Round(notStarted * 100m / total, 0),
                Math.Round(inProgress * 100m / total, 0))
            : new AdminStatusDto(0, 0, 0);

        var managers = await BuildManagerTableAsync(periodId, approvedSet, ct);
        var activity = await BuildActivityAsync(periodId, ct);

        return new AdminDashboardData(kpi, managers, deptProgress, timeline, rating, status, activity);
    }

    private static int Bucket(decimal final) => final switch
    {
        >= 90 => 4, >= 80 => 3, >= 60 => 2, >= 40 => 1, _ => 0
    };

    private async Task<IReadOnlyList<AdminManagerRowDto>> BuildManagerTableAsync(
        int periodId, HashSet<int> approvedSet, CancellationToken ct)
    {
        var pairs = await _db.Employees.AsNoTracking()
            .Where(e => e.IsActive && e.ReportingManagerId != null)
            .Select(e => new { ManagerId = e.ReportingManagerId!.Value, ReportId = e.Id })
            .ToListAsync(ct);
        if (pairs.Count == 0) return Array.Empty<AdminManagerRowDto>();

        var managerIds = pairs.Select(p => p.ManagerId).Distinct().ToList();
        var managers = await _db.Employees.AsNoTracking()
            .Where(e => managerIds.Contains(e.Id))
            .Select(e => new { e.Id, e.Name, Department = e.Department!.Name })
            .ToListAsync(ct);

        var byManager = pairs.GroupBy(p => p.ManagerId).ToDictionary(g => g.Key, g => g.Select(x => x.ReportId).ToList());

        var sr = 1;
        return managers
            .OrderBy(m => m.Name)
            .Select(m =>
            {
                var team = byManager.GetValueOrDefault(m.Id) ?? new List<int>();
                var done = team.Count(id => approvedSet.Contains(id));
                return new AdminManagerRowDto(sr++, m.Id, m.Name, m.Department, team.Count, done, team.Count - done);
            })
            .ToList();
    }

    private async Task<IReadOnlyList<AdminActivityDto>> BuildActivityAsync(int periodId, CancellationToken ct)
    {
        var events = new List<(DateTime When, int EmpId, string Desc)>();

        foreach (var r in await _db.PerReports.AsNoTracking()
                     .Where(r => r.EvaluationPeriodId == periodId && r.Approved && r.ApprovedOn != null)
                     .Select(r => new { r.EmployeeId, r.ApprovedOn }).ToListAsync(ct))
            events.Add((r.ApprovedOn!.Value, r.EmployeeId, "Completed PER submission"));

        foreach (var a in await _db.RatorAssignments.AsNoTracking()
                     .Where(a => a.EvaluationPeriodId == periodId)
                     .Select(a => new { a.RatorEmployeeId, a.CreatedAtUtc }).ToListAsync(ct))
            events.Add((a.CreatedAtUtc.UtcDateTime, a.RatorEmployeeId, "Assigned as rator"));

        foreach (var c in await _db.CompetencyRatings.AsNoTracking()
                     .Where(c => c.Status == RatingStatus.Done)
                     .Select(c => new { c.RatorEmployeeId, c.CreatedAtUtc, c.ModifiedAtUtc }).ToListAsync(ct))
            events.Add(((c.ModifiedAtUtc ?? c.CreatedAtUtc).UtcDateTime, c.RatorEmployeeId, "Submitted 360° evaluation"));

        foreach (var g in await _db.Goals.AsNoTracking()
                     .Where(g => g.EvaluationPeriodId == periodId)
                     .Select(g => new { g.EmployeeId, g.CreatedAtUtc }).ToListAsync(ct))
            events.Add((g.CreatedAtUtc.UtcDateTime, g.EmployeeId, "Submitted goals"));

        var top = events.OrderByDescending(e => e.When).Take(8).ToList();
        if (top.Count == 0) return Array.Empty<AdminActivityDto>();

        var ids = top.Select(e => e.EmpId).Distinct().ToList();
        var names = await _db.Employees.AsNoTracking()
            .Where(e => ids.Contains(e.Id)).ToDictionaryAsync(e => e.Id, e => e.Name, ct);

        return top
            .Select(e => new AdminActivityDto(names.GetValueOrDefault(e.EmpId, "Employee"), e.Desc, Ago(e.When)))
            .ToList();
    }

    private static string Ago(DateTime utc)
    {
        var span = DateTime.UtcNow - utc;
        if (span.TotalMinutes < 1) return "just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} minutes ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours} hours ago";
        return $"{(int)span.TotalDays} days ago";
    }

    public async Task<string?> SendNudgeAsync(int managerEmployeeId, CancellationToken ct = default)
        => await _db.Employees.Where(e => e.Id == managerEmployeeId).Select(e => e.Name).FirstOrDefaultAsync(ct);
}
