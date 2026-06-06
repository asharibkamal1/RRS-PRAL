using PralPer.Domain.Common;

namespace PralPer.Domain.Entities;

/// <summary>A single competency attribute (named AttributeItem to avoid clashing with System.Attribute).</summary>
public class AttributeItem : BaseEntity
{
    public int CompetencyId { get; set; }
    public Competency? Competency { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }       // shown under the attribute on the rating screen
    public decimal Weight { get; set; }            // 0.00–1.00 within its competency
    public bool IsActive { get; set; } = true;

    public ICollection<DesignationAttributeMap> DesignationMaps { get; set; } = new List<DesignationAttributeMap>();
}
