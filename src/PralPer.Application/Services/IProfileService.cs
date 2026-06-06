using PralPer.Domain.Entities;

namespace PralPer.Application.Services;

/// <summary>
/// Example application service contract (read-only employee profile).
/// Feature services live in the Application layer; UI pages depend on these interfaces,
/// not on repositories or the DbContext directly.
/// </summary>
public interface IProfileService
{
    Task<Employee?> GetByEmployeeIdAsync(int employeeId, CancellationToken ct = default);
    Task<IReadOnlyList<Employee>> GetAllActiveAsync(CancellationToken ct = default);
}
