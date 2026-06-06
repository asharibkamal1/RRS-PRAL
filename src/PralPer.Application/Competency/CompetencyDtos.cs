namespace PralPer.Application.Competency;

/// <summary>A ratee assigned to the current peer reviewer.</summary>
public sealed record RateeRowDto(int EmployeeId, string Name, string Department, string Status);

/// <summary>Editable attribute rating row (0–4) bound by the rating screen.</summary>
public sealed class AttributeRatingRow
{
    public int AttributeId { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public int Rating { get; set; }   // 0–4
}

/// <summary>A competency and its attributes (designation-driven) for one ratee.</summary>
public sealed record CompetencyGroup(string Competency, string Color, List<AttributeRatingRow> Attributes);

public sealed record DepartmentOption(int Id, string Name);

/// <summary>Employee option for the rater/ratee dropdowns (filled from the Employees table).</summary>
public sealed record EmployeeOption(int Id, string Name);

/// <summary>Details about the current peer reviewer (header card).</summary>
public sealed record ReviewerInfo(string Name, string JobTitle, string Department);
