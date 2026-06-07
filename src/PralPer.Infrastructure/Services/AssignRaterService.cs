using Microsoft.EntityFrameworkCore;
using PralPer.Application.Common;
using PralPer.Application.Raters;
using PralPer.Application.Services;
using PralPer.Domain.Entities;
using PralPer.Domain.Enums;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Services;

/// <summary>Reads/writes peer-rater (360°) assignments for the active evaluation period.</summary>
public sealed class AssignRaterService : IAssignRaterService
{
    private readonly AppDbContext _db;

    public AssignRaterService(AppDbContext db) => _db = db;

    private Task<int> ActivePeriodIdAsync(CancellationToken ct) =>
        _db.EvaluationPeriods.Where(p => p.Status == PeriodStatus.Active).Select(p => p.Id).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<RaterDeptOption>> GetDepartmentsAsync(CancellationToken ct = default)
        => await _db.Departments.OrderBy(d => d.Name)
            .Select(d => new RaterDeptOption(d.Id, d.Name)).ToListAsync(ct);

    public async Task<IReadOnlyList<RateeEmployeeDto>> GetEmployeesAsync(string? search, int? departmentId, CancellationToken ct = default)
    {
        var q = _db.Employees.AsNoTracking().Where(e => e.IsActive);
        if (departmentId is int dep)
            q = q.Where(e => e.DepartmentId == dep);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(e => e.Name.Contains(s));
        }

        return await q.OrderBy(e => e.Name)
            .Select(e => new RateeEmployeeDto(e.Id, e.Name, e.JobTitle ?? e.Designation!.Name, e.Department!.Name))
            .ToListAsync(ct);
    }

    public async Task<AssignmentViewDto?> GetAssignmentAsync(int rateeEmployeeId, CancellationToken ct = default)
    {
        var ratee = await _db.Employees.AsNoTracking()
            .Where(e => e.Id == rateeEmployeeId)
            .Select(e => new { e.Id, e.Name, JobTitle = e.JobTitle ?? e.Designation!.Name, Department = e.Department!.Name })
            .FirstOrDefaultAsync(ct);
        if (ratee is null) return null;

        var periodId = await ActivePeriodIdAsync(ct);

        var assigned = periodId == 0
            ? new HashSet<int>()
            : (await _db.RatorAssignments
                .Where(a => a.EvaluationPeriodId == periodId && a.RateeEmployeeId == rateeEmployeeId)
                .Select(a => a.RatorEmployeeId).ToListAsync(ct)).ToHashSet();

        var candidates = await _db.Employees.AsNoTracking()
            .Where(e => e.IsActive && e.Id != rateeEmployeeId)
            .OrderBy(e => e.Name)
            .Select(e => new { e.Id, e.Name, JobTitle = e.JobTitle ?? e.Designation!.Name, Department = e.Department!.Name })
            .ToListAsync(ct);

        var list = candidates
            .Select(c => new RaterCandidateDto(c.Id, c.Name, c.JobTitle, c.Department, assigned.Contains(c.Id)))
            .ToList();

        var header = new RateeHeaderDto(ratee.Id, ratee.Name, ratee.JobTitle, ratee.Department, assigned.Count);
        return new AssignmentViewDto(header, list);
    }

    public async Task<Result> SaveAssignmentsAsync(int rateeEmployeeId, IReadOnlyList<int> ratorIds, CancellationToken ct = default)
    {
        var periodId = await ActivePeriodIdAsync(ct);
        if (periodId == 0) return Result.Failure("No active evaluation period.");

        var wanted = ratorIds.Where(id => id != rateeEmployeeId).Distinct().ToHashSet();

        var existing = await _db.RatorAssignments
            .Where(a => a.EvaluationPeriodId == periodId && a.RateeEmployeeId == rateeEmployeeId)
            .ToListAsync(ct);

        // remove the ones no longer selected
        var toRemove = existing.Where(a => !wanted.Contains(a.RatorEmployeeId)).ToList();
        if (toRemove.Count > 0) _db.RatorAssignments.RemoveRange(toRemove);

        // add the newly selected ones
        var current = existing.Select(a => a.RatorEmployeeId).ToHashSet();
        foreach (var ratorId in wanted.Where(id => !current.Contains(id)))
            _db.RatorAssignments.Add(new RatorAssignment
            {
                EvaluationPeriodId = periodId,
                RateeEmployeeId = rateeEmployeeId,
                RatorEmployeeId = ratorId
            });

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<IReadOnlyList<RateeStatusRow>> GetRateeStatusAsync(CancellationToken ct = default)
    {
        var periodId = await ActivePeriodIdAsync(ct);

        var employees = await _db.Employees.AsNoTracking()
            .Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .Select(e => new { e.Id, e.Name, Department = e.Department!.Name })
            .ToListAsync(ct);

        var assignments = await _db.RatorAssignments
            .Where(a => a.EvaluationPeriodId == periodId)
            .Select(a => new { a.RateeEmployeeId, RatorName = a.RatorEmployee!.Name })
            .ToListAsync(ct);

        var byRatee = assignments
            .GroupBy(a => a.RateeEmployeeId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.RatorName).OrderBy(n => n).ToList());

        return employees.Select(e =>
        {
            var names = byRatee.GetValueOrDefault(e.Id) ?? new List<string>();
            return new RateeStatusRow(e.Id, e.Name, e.Department, names.Count > 0, names);
        }).ToList();
    }
}
