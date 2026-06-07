namespace PralPer.Application.Reports;

public sealed record PerReportGoalLineDto(
    string Title, decimal WeightPercent, decimal ProgressPercent, int Rating, int MaxRating, decimal ContributionPercent);

public sealed record PerReportCompetencyLineDto(string Competency, int Score, int MaxScore);

public sealed record PerReportDto(
    // header
    string EmployeeName,
    string JobTitle,
    string HrCode,
    string Department,
    string PayGroup,
    string PeriodName,
    bool Approved,
    string? ManagerName,
    // promotion & increment box
    DateTime? RecruitmentDate,
    DateTime? LastPromotionDate,
    DateTime? LastIncrementDate,
    int? LastIncrementBand,
    DateTime? LastBonusDate,
    int? LastBonusBand,
    // scores
    decimal GoalScorePercent,
    decimal CompetencyScorePercent,
    decimal GoalWeightPercent,
    decimal CompetencyWeightPercent,
    decimal FinalPercent,
    string Band,
    // breakdowns
    IReadOnlyList<PerReportGoalLineDto> Goals,
    IReadOnlyList<PerReportCompetencyLineDto> Competencies,
    // manager remarks
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> DevelopmentAreas,
    string? OverallComments,
    string? ApprovedBy,
    DateTime? ApprovedOn,
    // approval milestones
    DateTime? GoalsSubmittedOn,
    DateTime? Evaluation360On,
    DateTime? ManagerReviewOn,
    DateTime? FinalApprovalOn);
