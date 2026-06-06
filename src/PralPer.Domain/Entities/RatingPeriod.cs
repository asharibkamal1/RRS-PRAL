using PralPer.Domain.Common;
using PralPer.Domain.Enums;

namespace PralPer.Domain.Entities;

public class RatingPeriod : AuditableEntity
{
    public int EvaluationPeriodId { get; set; }
    public EvaluationPeriod? EvaluationPeriod { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public PeriodStatus Status { get; set; } = PeriodStatus.Active;
}
