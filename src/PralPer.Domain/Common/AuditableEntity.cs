namespace PralPer.Domain.Common;

/// <summary>Base type for entities that carry audit metadata.</summary>
public abstract class AuditableEntity : BaseEntity, IAuditableEntity
{
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
}
