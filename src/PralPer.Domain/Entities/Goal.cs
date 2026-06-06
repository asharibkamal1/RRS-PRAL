using PralPer.Domain.Common;

namespace PralPer.Domain.Entities;

/// <summary>
/// A performance goal (Section 2). Each employee submits 3–5 goals whose WeightPercent sum to 100.
/// Rating (1–10) is manager-assessed achievement; ProgressPercent is employee self-reported.
/// </summary>
public class Goal : AuditableEntity
{
    public int EvaluationPeriodId { get; set; }
    public EvaluationPeriod? EvaluationPeriod { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int GoalNo { get; set; }                 // 1..5
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public decimal ProgressPercent { get; set; }    // 0–100 (self-reported)
    public decimal WeightPercent { get; set; }      // 0–100 (Σ per employee = 100)
    public int Rating { get; set; }                 // 1–10 (manager-assessed)
}
