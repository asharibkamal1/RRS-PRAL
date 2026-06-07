using PralPer.Domain.Common;

namespace PralPer.Domain.Entities;

/// <summary>
/// Finalized PER report snapshot for an employee/period. All values are produced by the
/// database (dummy-seeded here) and only read by the app — no calculation client-side.
/// </summary>
public class PerReport : AuditableEntity
{
    public int EvaluationPeriodId { get; set; }
    public EvaluationPeriod? EvaluationPeriod { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    // Scores (already calculated, percentages)
    public decimal GoalScorePercent { get; set; }
    public decimal CompetencyScorePercent { get; set; }
    public decimal GoalWeightPercent { get; set; } = 70;
    public decimal CompetencyWeightPercent { get; set; } = 30;
    public decimal FinalPercent { get; set; }
    public string Band { get; set; } = string.Empty;          // e.g. "Excellent"

    public bool Approved { get; set; }
    public string? ManagerName { get; set; }

    // Manager remarks
    public string? Strengths { get; set; }                    // newline-separated bullets
    public string? DevelopmentAreas { get; set; }             // newline-separated bullets
    public string? OverallComments { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedOn { get; set; }

    // Approval milestones
    public DateTime? GoalsSubmittedOn { get; set; }
    public DateTime? Evaluation360On { get; set; }
    public DateTime? ManagerReviewOn { get; set; }
    public DateTime? FinalApprovalOn { get; set; }

    public ICollection<PerReportGoalLine> GoalLines { get; set; } = new List<PerReportGoalLine>();
    public ICollection<PerReportCompetencyLine> CompetencyLines { get; set; } = new List<PerReportCompetencyLine>();
}
