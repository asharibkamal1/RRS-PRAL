namespace PralPer.Application.Manager;

public sealed record DeptPerfDto(string Department, int Completed, int Pending);
public sealed record ApprovalDto(string EmployeeName, string ApprovalType, string AgoText);
public sealed record RaterRowDto(int RaterId, string Name, bool IsActiveRator, int Assigned, int Completed, int Pending);

public sealed record ManagerHomeDto(
    decimal TeamEvaluationProgress,
    int PendingApprovals,
    decimal AverageTeamScore,
    int EmployeesAwaitingReview,
    int TotalEvaluationsAssigned,
    int PendingEvaluations,
    int SubmittedEvaluations,
    int DaysUntilDeadline,
    IReadOnlyList<DeptPerfDto> Departments,
    IReadOnlyList<ApprovalDto> Approvals,
    IReadOnlyList<RaterRowDto> Raters);

// Popup: a rater's assigned ratees
public sealed record RateeRowDto(int SnapshotId, string RateeName, string Department, string Status);
public sealed record RaterEvaluationsDto(string RaterName, string RaterJobTitle, string PeriodName, IReadOnlyList<RateeRowDto> Ratees);

// Read-only 360 detail
public sealed record PeerCompetencyLineDto(string AttributeName, string? Remarks, int Score, int MaxScore);
public sealed record PeerCompetencyGroupDto(string Competency, string Color, IReadOnlyList<PeerCompetencyLineDto> Lines, decimal Average);
public sealed record PeerEvaluationDetailDto(
    string RateeName,
    string RateeJobTitle,
    string RaterName,
    string PeriodName,
    decimal AverageRating,
    int CompetenciesRated,
    decimal AssessmentScorePercent,
    IReadOnlyList<PeerCompetencyGroupDto> Groups);
