using PralPer.Application.Dashboards;

namespace PralPer.Application.Services;

/// <summary>
/// Backs the rich Administrator Dashboard. KPIs and chart data come from the seeded snapshot
/// tables; the Manager Evaluation table is computed live from real managers and their reports.
/// </summary>
public interface IAdminDashboardService
{
    Task<AdminDashboardData> GetAsync(CancellationToken ct = default);

    /// <summary>Records a "nudge sent" activity for the given manager (returns the manager name).</summary>
    Task<string?> SendNudgeAsync(int managerEmployeeId, CancellationToken ct = default);
}
