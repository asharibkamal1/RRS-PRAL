using PralPer.Domain.Common;

namespace PralPer.Domain.Entities;

public class IncrementHistory : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int Year { get; set; }
    public decimal Percentage { get; set; }
    public decimal Amount { get; set; }
}
