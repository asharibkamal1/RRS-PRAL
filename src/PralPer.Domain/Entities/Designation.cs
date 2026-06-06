using PralPer.Domain.Common;

namespace PralPer.Domain.Entities;

public class Designation : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<DesignationAttributeMap> AttributeMaps { get; set; } = new List<DesignationAttributeMap>();
}
