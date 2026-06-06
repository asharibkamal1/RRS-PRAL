using PralPer.Application.Common;
using PralPer.Application.Competency;

namespace PralPer.Application.Services;

/// <summary>
/// 360° Competency Rating (Section 3). Designation-driven attributes. The app inserts raw
/// peer ratings (0–4) and reads them back — scoring is performed by the database.
/// </summary>
public interface ICompetencyService
{
    Task<bool> IsRatingOpenAsync(CancellationToken ct = default);

    Task<ReviewerInfo?> GetReviewerAsync(int employeeId, CancellationToken ct = default);

    // Dropdowns filled from tables
    Task<IReadOnlyList<EmployeeOption>> GetRatersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DepartmentOption>> GetDepartmentsAsync(CancellationToken ct = default);

    /// <summary>Ratees assigned to the given rater, optionally filtered by department.</summary>
    Task<IReadOnlyList<RateeRowDto>> GetRateesAsync(int ratorEmployeeId, int? departmentId = null, CancellationToken ct = default);

    /// <summary>Designation-mapped attributes for a ratee, grouped by competency, with any saved ratings.</summary>
    Task<IReadOnlyList<CompetencyGroup>> GetRateeAttributesAsync(int ratorEmployeeId, int rateeEmployeeId, CancellationToken ct = default);

    /// <summary>Inserts/updates the raw peer ratings; marks Done when submitted.</summary>
    Task<Result> SaveRatingsAsync(int ratorEmployeeId, int rateeEmployeeId, IReadOnlyList<AttributeRatingRow> rows, bool submit, CancellationToken ct = default);
}
