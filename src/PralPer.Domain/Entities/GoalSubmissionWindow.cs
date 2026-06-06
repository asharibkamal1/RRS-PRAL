using PralPer.Domain.Common;
using PralPer.Domain.Enums;

namespace PralPer.Domain.Entities;

public class GoalSubmissionWindow : AuditableEntity
{
    public int EvaluationPeriodId { get; set; }
    public EvaluationPeriod? EvaluationPeriod { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool AllowSubmission { get; set; }

    // Optional restriction scope
    public int? RestrictDepartmentId { get; set; }
    public Department? RestrictDepartment { get; set; }
    public int? RestrictEmployeeId { get; set; }
    public Employee? RestrictEmployee { get; set; }

    public PeriodStatus Status { get; set; } = PeriodStatus.Active;
}
