using PralPer.Domain.Common;

namespace PralPer.Domain.Entities;

/// <summary>Maps a competency attribute to a designation (drives designation-sensitive 360° evaluation).</summary>
public class DesignationAttributeMap : BaseEntity
{
    public int DesignationId { get; set; }
    public Designation? Designation { get; set; }

    public int AttributeId { get; set; }
    public AttributeItem? Attribute { get; set; }

    public decimal Weight { get; set; }            // 0.00–1.00
}
