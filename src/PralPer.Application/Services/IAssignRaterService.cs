using PralPer.Application.Common;
using PralPer.Application.Raters;

namespace PralPer.Application.Services;

/// <summary>
/// Backs the manager's Assign Raters screen: pick a ratee, toggle the peer raters who will give
/// 360° feedback, and persist the selection as RatorAssignment rows for the active period.
/// </summary>
public interface IAssignRaterService
{
    Task<IReadOnlyList<RaterDeptOption>> GetDepartmentsAsync(CancellationToken ct = default);

    /// <summary>All active employees (optional search + department filter) — the ratee candidates.</summary>
    Task<IReadOnlyList<RateeEmployeeDto>> GetEmployeesAsync(string? search, int? departmentId, CancellationToken ct = default);

    /// <summary>The ratee header plus every candidate rater with its current assigned state.</summary>
    Task<AssignmentViewDto?> GetAssignmentAsync(int rateeEmployeeId, CancellationToken ct = default);

    /// <summary>Replaces the ratee's peer raters for the active period with the given set.</summary>
    Task<Result> SaveAssignmentsAsync(int rateeEmployeeId, IReadOnlyList<int> ratorIds, CancellationToken ct = default);

    /// <summary>Assignment status per ratee (Done = has at least one rater) with the assigned rater names.</summary>
    Task<IReadOnlyList<RateeStatusRow>> GetRateeStatusAsync(CancellationToken ct = default);
}
