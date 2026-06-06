using PralPer.Domain.Entities;

namespace PralPer.Application.Profiles;

public sealed record PromotionDto(DateTime Date, string? FromTitle, string ToTitle, string? Note);
public sealed record IncrementDto(int Year, decimal Percentage, decimal Amount);
public sealed record BonusDto(int Year, string Type, string Quarter, decimal Amount);

/// <summary>Full read-only profile view (employee master + HRMS history + active period).</summary>
public sealed record EmployeeProfileDto(
    Employee Employee,
    string CurrentPeriodName,
    IReadOnlyList<PromotionDto> Promotions,
    IReadOnlyList<IncrementDto> Increments,
    IReadOnlyList<BonusDto> Bonuses);
