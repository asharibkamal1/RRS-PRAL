using Microsoft.EntityFrameworkCore;
using PralPer.Application.Dashboards;
using PralPer.Application.Services;
using PralPer.Domain.Entities;
using PralPer.Domain.Enums;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Services;

/// <summary>Reads the admin dashboard snapshot and builds the live manager evaluation table.</summary>
public sealed class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _db;

    public AdminDashboardService(AppDbContext db) => _db = db;

    public async Task<AdminDashboardData> GetAsync(CancellationToken ct = default)
    {
        var stat = await _db.AdminDashboardStats.AsNoTracking().FirstOrDefaultAsync(ct);
        var kpi = stat is null
            ? new AdminKpiDto(0, 0, 0, 0, 0, 0, "—", 0)
            : new AdminKpiDto(stat.TotalEmployees, stat.EmployeesTrend, stat.EvaluationsPending, stat.PendingTrend,
                stat.CompletedReviews, stat.CompletedTrend, stat.ActiveCycleName, stat.ActiveCyclePercent);
        var status = stat is null
            ? new AdminStatusDto(0, 0, 0)
            : new AdminStatusDto(stat.StatusCompletedPercent, stat.StatusNotStartedPercent, stat.StatusInProgressPercent);

        var series = await _db.AdminDashboardSeriesPoints.AsNoTracking().OrderBy(s => s.SortOrder).ToListAsync(ct);

        var dept = series.Where(s => s.Category == "Dept")
            .Select(s => new AdminDeptProgressDto(s.Label, (int)s.ValueA, (int)s.ValueB)).ToList();
        var timeline = series.Where(s => s.Category == "Timeline")
            .Select(s => new AdminTimelinePointDto(s.Label, (double)s.ValueA)).ToList();
        var rating = series.Where(s => s.Category == "Rating")
            .Select(s => new AdminRatingBucketDto(s.SortOrder, s.Label, s.ValueA)).ToList();

        var activity = await _db.AdminActivities.AsNoTracking()
            .OrderBy(a => a.SortOrder)
            .Select(a => new AdminActivityDto(a.ActorName, a.Description, a.AgoText))
            .ToListAsync(ct);

        var managers = await BuildManagerTableAsync(ct);

        return new AdminDashboardData(kpi, managers, dept, timeline, rating, status, activity);
    }

    private async Task<IReadOnlyList<AdminManagerRowDto>> BuildManagerTableAsync(CancellationToken ct)
    {
        var periodId = await _db.EvaluationPeriods
            .Where(p => p.Status == PeriodStatus.Active).Select(p => p.Id).FirstOrDefaultAsync(ct);

        // every (manager, report) pair
        var pairs = await _db.Employees.AsNoTracking()
            .Where(e => e.IsActive && e.ReportingManagerId != null)
            .Select(e => new { ManagerId = e.ReportingManagerId!.Value, ReportId = e.Id })
            .ToListAsync(ct);
        if (pairs.Count == 0) return Array.Empty<AdminManagerRowDto>();

        var completedEmpIds = (await _db.PerReports.AsNoTracking()
            .Where(r => r.EvaluationPeriodId == periodId)
            .Select(r => r.EmployeeId).ToListAsync(ct)).ToHashSet();

        var managerIds = pairs.Select(p => p.ManagerId).Distinct().ToList();
        var managers = await _db.Employees.AsNoTracking()
            .Where(e => managerIds.Contains(e.Id))
            .Select(e => new { e.Id, e.Name, Department = e.Department!.Name })
            .ToListAsync(ct);

        var byManager = pairs.GroupBy(p => p.ManagerId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ReportId).ToList());

        var sr = 1;
        return managers
            .OrderBy(m => m.Name)
            .Select(m =>
            {
                var team = byManager.GetValueOrDefault(m.Id) ?? new List<int>();
                var completed = team.Count(id => completedEmpIds.Contains(id));
                return new AdminManagerRowDto(sr++, m.Id, m.Name, m.Department, team.Count, completed, team.Count - completed);
            })
            .ToList();
    }

    public async Task<string?> SendNudgeAsync(int managerEmployeeId, CancellationToken ct = default)
    {
        var name = await _db.Employees.Where(e => e.Id == managerEmployeeId).Select(e => e.Name).FirstOrDefaultAsync(ct);
        if (name is null) return null;

        _db.AdminActivities.Add(new AdminActivity
        {
            SortOrder = -1,
            ActorName = "Admin",
            Description = $"Sent an evaluation reminder to {name}",
            AgoText = "just now"
        });
        await _db.SaveChangesAsync(ct);
        return name;
    }
}
