namespace PralPer.Application.Dashboards;

// Read models for the rich admin dashboard. KPIs/charts come from the seeded snapshot tables;
// the manager evaluation table is built live from the Employees table.

public sealed record AdminKpiDto(
    int TotalEmployees, decimal? EmployeesTrend,
    int EvaluationsPending, decimal? PendingTrend,
    int CompletedReviews, decimal? CompletedTrend,
    string ActiveCycleName, decimal ActiveCyclePercent);

/// <summary>One manager row in the Manager Evaluation table.</summary>
public sealed record AdminManagerRowDto(
    int Sr, int ManagerId, string ManagerName, string Department, int TeamSize, int Completed, int Pending);

public sealed record AdminDeptProgressDto(string Department, int Completed, int Pending);

public sealed record AdminTimelinePointDto(string Label, double Value);

public sealed record AdminRatingBucketDto(int Score, string Label, decimal Percent);

public sealed record AdminStatusDto(decimal CompletedPercent, decimal NotStartedPercent, decimal InProgressPercent);

public sealed record AdminActivityDto(string ActorName, string Description, string AgoText);

public sealed record AdminDashboardData(
    AdminKpiDto Kpi,
    IReadOnlyList<AdminManagerRowDto> Managers,
    IReadOnlyList<AdminDeptProgressDto> DeptProgress,
    IReadOnlyList<AdminTimelinePointDto> Timeline,
    IReadOnlyList<AdminRatingBucketDto> Rating,
    AdminStatusDto Status,
    IReadOnlyList<AdminActivityDto> Activity);
