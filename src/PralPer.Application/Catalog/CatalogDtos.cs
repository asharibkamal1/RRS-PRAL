namespace PralPer.Application.Catalog;

/// <summary>Competency option for the "Select Competency" dropdowns (from the Competencies table).</summary>
public sealed record CompetencyOption(int Id, string Name);

/// <summary>Job level option for the "Select Level" dropdown (from the Designations table).</summary>
public sealed record LevelOption(int Id, string Name);

/// <summary>A competency with how many attributes it has — drives the summary count cards.</summary>
public sealed record CompetencyCountDto(int Id, string Name, int AttributeCount);

/// <summary>One attribute row in the Attribute Administration (Competency Management) table.</summary>
public sealed record AttributeAdminRow(
    int Id, int CompetencyId, string Competency, string Name, string? Description, decimal Weight, bool IsActive);

/// <summary>Input for adding a new competency attribute.</summary>
public sealed class AttributeInput
{
    public int CompetencyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Weight { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>One attribute→level mapping row in the Level Mapping (Attribute Mapping) table.</summary>
public sealed record LevelMapRow(int Id, string Competency, string Attribute, string Level, decimal Weight);

/// <summary>Per-competency total mapped weight vs 100% — drives the Competency Weightage Summary cards.</summary>
public sealed record CompetencyWeightDto(string Competency, decimal TotalWeight);
