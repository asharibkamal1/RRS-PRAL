using PralPer.Domain.Common;

namespace PralPer.Domain.Entities;

public class PromotionHistory : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime EffectiveDate { get; set; }
    public string? FromTitle { get; set; }
    public string ToTitle { get; set; } = string.Empty;
    public string? Note { get; set; }
}
