namespace PralPer.Domain.Common;

/// <summary>Marks an entity whose create/modify audit fields are stamped automatically.</summary>
public interface IAuditableEntity
{
    DateTimeOffset CreatedAtUtc { get; set; }
    string? CreatedBy { get; set; }
    DateTimeOffset? ModifiedAtUtc { get; set; }
    string? ModifiedBy { get; set; }
}
