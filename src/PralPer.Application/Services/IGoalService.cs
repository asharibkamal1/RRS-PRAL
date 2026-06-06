using PralPer.Application.Common;
using PralPer.Application.Goals;

namespace PralPer.Application.Services;

/// <summary>
/// Goal Submission (Section 2). The app only inserts/fetches raw goal data — no scoring.
/// </summary>
public interface IGoalService
{
    /// <summary>Existing saved goals for the employee in the active period (empty if none).</summary>
    Task<IReadOnlyList<GoalView>> GetMyGoalsAsync(int employeeId, CancellationToken ct = default);

    /// <summary>True when the goal-submission window is currently open.</summary>
    Task<bool> IsSubmissionOpenAsync(CancellationToken ct = default);

    /// <summary>Validates (3–5 goals, weights total 100%) and persists the raw goal rows.</summary>
    Task<Result> SaveMyGoalsAsync(int employeeId, IReadOnlyList<GoalInput> goals, CancellationToken ct = default);
}
