namespace PralPer.Application.Dashboards;

// Read models for dashboards. Values originate from the database (pre-calculated by SPs);
// the app only fetches and displays them.

public sealed record GoalProgressDto(string Title, decimal WeightPercent, decimal ProgressPercent);

public sealed record PeerEvaluationDto(string EmployeeName, string Designation, string Department, string Status);

public sealed record EmployeeDashboardDto(
    decimal GoalCompletionPercent,
    string EvaluationStatus,
    decimal FinalPerScore,
    int PendingActions,
    int PendingEvaluations,
    int SubmittedEvaluations,
    int DaysUntilDeadline,
    DateTime? EvaluationDeadline,
    string? ManagerStrengths,
    string? ManagerDevelopmentAreas,
    string? ManagerName,
    IReadOnlyList<GoalProgressDto> Goals,
    IReadOnlyList<PeerEvaluationDto> PendingList,
    IReadOnlyList<PeerEvaluationDto> CompletedList);

public sealed record AdminDashboardDto(
    int TotalEmployees,
    int Completed,
    int Pending,
    int NotStarted,
    string ActiveCycleName);

public sealed record ManagerDashboardDto(
    string ActiveCycleName,
    int TeamMembers,
    int GoalsToReview,
    int PendingRatings,
    int ApprovedPers,
    IReadOnlyList<PeerEvaluationDto> Team);
