using PralPer.Domain.Common;

namespace PralPer.Domain.Entities;

/// <summary>
/// Pre-computed dashboard/evaluation figures for an employee in a period.
/// ALL values here are produced by the database (stored procedures); the app only
/// reads them. The app never computes scores client-side.
/// </summary>
public class EmployeeEvaluationSummary : AuditableEntity
{
    public int EvaluationPeriodId { get; set; }
    public EvaluationPeriod? EvaluationPeriod { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    // KPI cards (already calculated in DB)
    public decimal GoalCompletionPercent { get; set; }   // e.g. 85
    public string EvaluationStatus { get; set; } = "Not Started"; // e.g. "In Review"
    public decimal FinalPerScore { get; set; }            // e.g. 82.8
    public int PendingActions { get; set; }

    // Peer-rating overview
    public int PendingEvaluations { get; set; }
    public int SubmittedEvaluations { get; set; }
    public int DaysUntilDeadline { get; set; }
    public DateTime? EvaluationDeadline { get; set; }

    // Manager feedback (read-only, populated by DB/Manager phase)
    public string? ManagerStrengths { get; set; }
    public string? ManagerDevelopmentAreas { get; set; }
    public string? ManagerName { get; set; }
    public DateTime? FeedbackUpdatedOn { get; set; }
}
