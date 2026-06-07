using PralPer.Application.Reports;

namespace PralPer.Application.Services;

/// <summary>Fetches the finalized (pre-calculated) PER report for an employee. Read-only.</summary>
public interface IPerReportService
{
    Task<PerReportDto?> GetReportAsync(int employeeId, CancellationToken ct = default);
}
