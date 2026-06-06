using PralPer.Domain.Common;

namespace PralPer.Domain.Entities;

public class BonusHistory : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int Year { get; set; }
    public string BonusType { get; set; } = string.Empty;
    public string Quarter { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
