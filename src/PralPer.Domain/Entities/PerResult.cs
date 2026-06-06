using PralPer.Domain.Common;

namespace PralPer.Domain.Entities;

/// <summary>Computed/cached final PER result for an employee within an evaluation period.</summary>
public class PerResult : AuditableEntity
{
    public int EvaluationPeriodId { get; set; }
    public EvaluationPeriod? EvaluationPeriod { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public decimal GoalScore { get; set; }          // 0–10
    public decimal PeerScore { get; set; }          // 0–10
    public decimal FinalScore { get; set; }         // 0–10
    public DateTimeOffset GeneratedAtUtc { get; set; }
}
