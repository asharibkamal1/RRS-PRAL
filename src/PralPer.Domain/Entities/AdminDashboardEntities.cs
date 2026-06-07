using PralPer.Domain.Common;

namespace PralPer.Domain.Entities;

/// <summary>Scalar KPIs + status-distribution for the admin dashboard (pre-calculated, dummy-seeded).</summary>
public class AdminDashboardStat : BaseEntity
{
    public int TotalEmployees { get; set; }
    public decimal EmployeesTrend { get; set; }          // % change vs last period (can be negative)
    public int EvaluationsPending { get; set; }
    public decimal PendingTrend { get; set; }
    public int CompletedReviews { get; set; }
    public decimal CompletedTrend { get; set; }
    public string ActiveCycleName { get; set; } = string.Empty;
    public decimal ActiveCyclePercent { get; set; }

    // Status Distribution pie
    public decimal StatusCompletedPercent { get; set; }
    public decimal StatusNotStartedPercent { get; set; }
    public decimal StatusInProgressPercent { get; set; }
}

/// <summary>
/// A generic data point backing the admin dashboard charts. Category groups the series:
/// "Dept" (ValueA=Completed, ValueB=Pending), "Timeline" (ValueA=value), "Rating" (SortOrder=score, ValueA=percent).
/// </summary>
public class AdminDashboardSeriesPoint : BaseEntity
{
    public string Category { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal ValueA { get; set; }
    public decimal ValueB { get; set; }
}

/// <summary>A row in the admin dashboard's Recent Activity feed.</summary>
public class AdminActivity : BaseEntity
{
    public int SortOrder { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AgoText { get; set; } = string.Empty;       // e.g. "2 minutes ago"
}
