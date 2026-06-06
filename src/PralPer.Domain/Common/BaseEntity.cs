namespace PralPer.Domain.Common;

/// <summary>Base type for all persistent entities (integer surrogate key).</summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
}
