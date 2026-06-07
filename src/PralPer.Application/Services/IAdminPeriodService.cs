using PralPer.Application.Admin;
using PralPer.Application.Common;

namespace PralPer.Application.Services;

/// <summary>
/// Admin setup screens: Evaluation Period, Rating Period and Goal Submission management.
/// Single-active model — creating or selecting a period activates it and archives the rest.
/// </summary>
public interface IAdminPeriodService
{
    // Evaluation Period
    Task<IReadOnlyList<PeriodRowDto>> GetEvaluationPeriodsAsync(CancellationToken ct = default);
    Task<Result> CreateEvaluationPeriodAsync(DateTime start, DateTime end, CancellationToken ct = default);
    Task<Result> ActivateEvaluationPeriodAsync(int id, CancellationToken ct = default);

    // Rating Period
    Task<IReadOnlyList<PeriodRowDto>> GetRatingPeriodsAsync(CancellationToken ct = default);
    Task<Result> CreateRatingPeriodAsync(DateTime start, DateTime end, CancellationToken ct = default);
    Task<Result> ActivateRatingPeriodAsync(int id, CancellationToken ct = default);

    // Goal Submission
    Task<IReadOnlyList<GoalWindowRowDto>> GetGoalWindowsAsync(CancellationToken ct = default);
    Task<Result> CreateGoalWindowAsync(DateTime start, DateTime end, bool allowSubmission,
        int? restrictDepartmentId, int? restrictEmployeeId, CancellationToken ct = default);
    Task<Result> ActivateGoalWindowAsync(int id, CancellationToken ct = default);

    // Dropdowns
    Task<IReadOnlyList<AdminDeptOption>> GetDepartmentsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AdminEmployeeOption>> SearchEmployeesAsync(string? term, CancellationToken ct = default);
}
