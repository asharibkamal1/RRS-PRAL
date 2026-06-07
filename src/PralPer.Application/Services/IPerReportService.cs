using PralPer.Application.Reports;

namespace PralPer.Application.Services;

/// <summary>Fetches the finalized (pre-calculated) PER report for an employee. Read-only.</summary>
public interface IPerReportService
{
    Task<PerReportDto?> GetReportAsync(int employeeId, CancellationToken ct = default);

    /// <summary>All finalized PER reports for the active period (manager list view).</summary>
    Task<IReadOnlyList<PerReportSummaryDto>> GetTeamReportsAsync(CancellationToken ct = default);
}
