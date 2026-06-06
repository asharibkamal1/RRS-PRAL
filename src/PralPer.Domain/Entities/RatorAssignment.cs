using PralPer.Domain.Common;

namespace PralPer.Domain.Entities;

/// <summary>Assigns a peer reviewer (Rator) to an employee being evaluated (Ratee) for 360° rating.</summary>
public class RatorAssignment : AuditableEntity
{
    public int EvaluationPeriodId { get; set; }
    public EvaluationPeriod? EvaluationPeriod { get; set; }

    public int RateeEmployeeId { get; set; }
    public Employee? RateeEmployee { get; set; }

    public int RatorEmployeeId { get; set; }
    public Employee? RatorEmployee { get; set; }
}
