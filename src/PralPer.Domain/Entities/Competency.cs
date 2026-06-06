using PralPer.Domain.Common;

namespace PralPer.Domain.Entities;

public class Competency : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<AttributeItem> Attributes { get; set; } = new List<AttributeItem>();
}
