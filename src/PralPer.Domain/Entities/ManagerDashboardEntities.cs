using PralPer.Domain.Common;

namespace PralPer.Domain.Entities;

/// <summary>Scalar KPIs for the manager dashboard (pre-calculated, dummy-seeded).</summary>
public class ManagerDashboardStat : BaseEntity
{
    public decimal TeamEvaluationProgress { get; set; }
    public int PendingApprovals { get; set; }
    public decimal AverageTeamScore { get; set; }
    public int EmployeesAwaitingReview { get; set; }
    public int TotalEvaluationsAssigned { get; set; }
    public int PendingEvaluations { get; set; }
    public int SubmittedEvaluations { get; set; }
    public int DaysUntilDeadline { get; set; }
}

/// <summary>Per-department completed/pending counts for the Team Performance Overview chart.</summary>
public class DeptPerformance : BaseEntity
{
    public int SortOrder { get; set; }
    public string Department { get; set; } = string.Empty;
    public int Completed { get; set; }
    public int Pending { get; set; }
}

/// <summary>A row in the manager's Pending Approvals list.</summary>
public class ManagerApproval : BaseEntity
{
    public int SortOrder { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string ApprovalType { get; set; } = string.Empty;
    public string AgoText { get; set; } = string.Empty;       // e.g. "2 hours ago"
}

/// <summary>A rater shown in the manager's Competency Rating Assignment table.</summary>
public class ManagerRater : BaseEntity
{
    public int SortOrder { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActiveRator { get; set; }

    public ICollection<PeerEvaluationSnapshot> Evaluations { get; set; } = new List<PeerEvaluationSnapshot>();
}
