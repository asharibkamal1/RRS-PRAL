using PralPer.Domain.Common;
using PralPer.Domain.Enums;

namespace PralPer.Domain.Entities;

public class EvaluationPeriod : AuditableEntity
{
    public string Name { get; set; } = string.Empty;        // e.g. "Q1 2026 (Jan - Mar)"
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public PeriodStatus Status { get; set; } = PeriodStatus.Active;

    public ICollection<RatingPeriod> RatingPeriods { get; set; } = new List<RatingPeriod>();
    public ICollection<GoalSubmissionWindow> GoalWindows { get; set; } = new List<GoalSubmissionWindow>();
}
