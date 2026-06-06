using Microsoft.EntityFrameworkCore;
using PralPer.Application.Profiles;
using PralPer.Application.Services;
using PralPer.Domain.Entities;
using PralPer.Domain.Enums;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Services;

/// <summary>Reads employee master data + HRMS history for the read-only Profile screen.</summary>
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

    public async Task<EmployeeProfileDto?> GetProfileAsync(int employeeId, CancellationToken ct = default)
    {
        var employee = await GetByEmployeeIdAsync(employeeId, ct);
        if (employee is null) return null;

        var periodName = await _db.EvaluationPeriods
            .Where(p => p.Status == PeriodStatus.Active)
            .Select(p => p.Name)
            .FirstOrDefaultAsync(ct) ?? "—";

        var promotions = await _db.PromotionHistories
            .Where(p => p.EmployeeId == employeeId)
            .OrderByDescending(p => p.EffectiveDate)
            .Select(p => new PromotionDto(p.EffectiveDate, p.FromTitle, p.ToTitle, p.Note))
            .AsNoTracking().ToListAsync(ct);

        var increments = await _db.IncrementHistories
            .Where(i => i.EmployeeId == employeeId)
            .OrderByDescending(i => i.Year)
            .Select(i => new IncrementDto(i.Year, i.Percentage, i.Amount))
            .AsNoTracking().ToListAsync(ct);

        var bonuses = await _db.BonusHistories
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.Year)
            .Select(x => new BonusDto(x.Year, x.BonusType, x.Quarter, x.Amount))
            .AsNoTracking().ToListAsync(ct);

        return new EmployeeProfileDto(employee, periodName, promotions, increments, bonuses);
    }
}
