using PralPer.Application.Common;
using PralPer.Application.Goals;

namespace PralPer.Application.Services;

/// <summary>One of the manager's direct reports, for the Team Member dropdown.</summary>
public sealed record TeamMemberDto(int EmployeeId, string Name, string? JobTitle);

/// <summary>
/// Goal Assessment (manager side). The manager picks a team member, sees the goals they
/// submitted, and assesses them by adjusting Weight % and the 0–4 Rating.
/// </summary>
public interface IManagerGoalService
{
    /// <summary>Direct reports of the given manager (by employee id), ordered by name.</summary>
    Task<IReadOnlyList<TeamMemberDto>> GetTeamAsync(int managerEmployeeId, CancellationToken ct = default);

    /// <summary>The team member's submitted goals for the active period.</summary>
    Task<IReadOnlyList<GoalView>> GetGoalsAsync(int employeeId, CancellationToken ct = default);

    /// <summary>Validates the weight total (=100%) and saves the manager's Weight + Rating changes.</summary>
    Task<Result> SaveAssessmentAsync(int employeeId, IReadOnlyList<GoalInput> goals, CancellationToken ct = default);
}
