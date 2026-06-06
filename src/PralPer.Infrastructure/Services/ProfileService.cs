using Microsoft.EntityFrameworkCore;
using PralPer.Application.Services;
using PralPer.Domain.Entities;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Services;

/// <summary>Reads employee master data (with org navigations) for the read-only Profile screen.</summary>
public sealed class ProfileService : IProfileService
{
    private readonly AppDbContext _db;

    public ProfileService(AppDbContext db) => _db = db;

    public Task<Employee?> GetByEmployeeIdAsync(int employeeId, CancellationToken ct = default)
        => _db.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.ReportingManager)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employeeId, ct);

    public async Task<IReadOnlyList<Employee>> GetAllActiveAsync(CancellationToken ct = default)
        => await _db.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Where(e => e.IsActive)
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .ToListAsync(ct);
}
