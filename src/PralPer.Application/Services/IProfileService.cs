using PralPer.Application.Profiles;
using PralPer.Domain.Entities;

namespace PralPer.Application.Services;

/// <summary>
/// Read-only employee profile data (master record + HRMS history). No data entry here.
/// </summary>
public interface IProfileService
{
    Task<Employee?> GetByEmployeeIdAsync(int employeeId, CancellationToken ct = default);
    Task<IReadOnlyList<Employee>> GetAllActiveAsync(CancellationToken ct = default);

    /// <summary>Full profile view including promotion/increment/bonus history and the active period.</summary>
    Task<EmployeeProfileDto?> GetProfileAsync(int employeeId, CancellationToken ct = default);
}
