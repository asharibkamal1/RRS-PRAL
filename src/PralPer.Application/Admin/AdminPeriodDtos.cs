namespace PralPer.Application.Admin;

/// <summary>A row in the Evaluation / Rating period records table.</summary>
public sealed record PeriodRowDto(int Id, DateTime StartDate, DateTime EndDate, bool IsActive);

/// <summary>A row in the Goal Submission archives table.</summary>
public sealed record GoalWindowRowDto(
    int Id, DateTime StartDate, DateTime EndDate, bool AllowSubmission,
    string? RestrictDepartment, string? RestrictEmployee);

public sealed record AdminDeptOption(int Id, string Name);

public sealed record AdminEmployeeOption(int Id, string Name);
