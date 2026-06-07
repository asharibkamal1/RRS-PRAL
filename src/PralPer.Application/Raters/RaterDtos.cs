namespace PralPer.Application.Raters;

/// <summary>Department option for the "All Departments" filter (from the Departments table).</summary>
public sealed record RaterDeptOption(int Id, string Name);

/// <summary>An employee that can be selected as a ratee (left list + bottom table).</summary>
public sealed record RateeEmployeeDto(int Id, string Name, string? JobTitle, string Department);

/// <summary>An available peer rater for the selected ratee, with whether they are currently assigned.</summary>
public sealed record RaterCandidateDto(int Id, string Name, string? JobTitle, string Department, bool IsAssigned);

/// <summary>Header card details for the selected ratee.</summary>
public sealed record RateeHeaderDto(int Id, string Name, string? JobTitle, string Department, int AssignedCount);

/// <summary>The selected ratee plus every candidate rater (assigned flag set).</summary>
public sealed record AssignmentViewDto(RateeHeaderDto Ratee, IReadOnlyList<RaterCandidateDto> Candidates);

/// <summary>One row in the bottom assignment-status table.</summary>
public sealed record RateeStatusRow(int Id, string Name, string Department, bool HasRaters, IReadOnlyList<string> RaterNames);
