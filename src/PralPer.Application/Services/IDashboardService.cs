using PralPer.Application.Dashboards;

namespace PralPer.Application.Services;

/// <summary>
/// Fetches pre-calculated dashboard data from the database for each role.
/// No scoring happens here — values come from DB tables / stored procedures.
/// </summary>
public interface IDashboardService
{
    Task<EmployeeDashboardDto?> GetEmployeeDashboardAsync(int employeeId, CancellationToken ct = default);
    Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct = default);
    Task<ManagerDashboardDto> GetManagerDashboardAsync(int managerEmployeeId, CancellationToken ct = default);
}
