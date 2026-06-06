using PralPer.Domain.Common;
using PralPer.Domain.Enums;

namespace PralPer.Domain.Entities;

/// <summary>A single peer (360°) rating: one rator rates one ratee on one attribute (1–10).</summary>
public class CompetencyRating : AuditableEntity
{
    public int RatingPeriodId { get; set; }
    public RatingPeriod? RatingPeriod { get; set; }

    public int RateeEmployeeId { get; set; }
    public Employee? RateeEmployee { get; set; }

    public int RatorEmployeeId { get; set; }
    public Employee? RatorEmployee { get; set; }

    public int AttributeId { get; set; }
    public AttributeItem? Attribute { get; set; }

    public int Rating { get; set; }                 // 1–10
    public string? Remarks { get; set; }
    public RatingStatus Status { get; set; } = RatingStatus.Pending;
}
