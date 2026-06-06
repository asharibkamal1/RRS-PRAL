using Microsoft.EntityFrameworkCore;
using PralPer.Application.Common;
using PralPer.Application.Competency;
using PralPer.Application.Services;
using PralPer.Domain.Entities;
using PralPer.Domain.Enums;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Services;

/// <summary>
/// 360° peer rating data access. Reads designation-mapped attributes and saves raw ratings.
/// No scoring here — the database computes peer/PER scores.
/// </summary>
public sealed class CompetencyService : ICompetencyService
{
    private readonly AppDbContext _db;

    public CompetencyService(AppDbContext db) => _db = db;

    private static readonly Dictionary<string, string> CompetencyColors = new()
    {
        ["Integrity"] = "#D97706",
        ["Team Work & Collaboration"] = "#16A34A",
        ["Problem Solving & Innovation"] = "#DC2626",
        ["Takes Ownership"] = "#7C3AED",
        ["Leadership"] = "#2563EB",
    };

    private Task<int> ActiveEvalPeriodIdAsync(CancellationToken ct) =>
        _db.EvaluationPeriods.Where(p => p.Status == PeriodStatus.Active).Select(p => p.Id).FirstOrDefaultAsync(ct);

    private Task<int> ActiveRatingPeriodIdAsync(CancellationToken ct) =>
        _db.RatingPeriods.Where(p => p.Status == PeriodStatus.Active).Select(p => p.Id).FirstOrDefaultAsync(ct);

    public async Task<bool> IsRatingOpenAsync(CancellationToken ct = default)
        => await ActiveRatingPeriodIdAsync(ct) != 0;

    public Task<ReviewerInfo?> GetReviewerAsync(int employeeId, CancellationToken ct = default)
        => _db.Employees
            .Where(e => e.Id == employeeId)
            .Select(e => new ReviewerInfo(e.Name, e.JobTitle ?? e.Designation!.Name, e.Department!.Name))
            .FirstOrDefaultAsync(ct)!;

    public async Task<IReadOnlyList<EmployeeOption>> GetRatersAsync(CancellationToken ct = default)
        => await _db.Employees.Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .Select(e => new EmployeeOption(e.Id, e.Name))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<DepartmentOption>> GetDepartmentsAsync(CancellationToken ct = default)
        => await _db.Departments
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentOption(d.Id, d.Name))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<RateeRowDto>> GetRateesAsync(int ratorEmployeeId, int? departmentId = null, CancellationToken ct = default)
    {
        var periodId = await ActiveEvalPeriodIdAsync(ct);
        if (periodId == 0) return Array.Empty<RateeRowDto>();

        var ratees = await _db.RatorAssignments
            .Where(a => a.EvaluationPeriodId == periodId && a.RatorEmployeeId == ratorEmployeeId)
            .Select(a => new
            {
                a.RateeEmployeeId,
                a.RateeEmployee!.Name,
                a.RateeEmployee!.DepartmentId,
                Department = a.RateeEmployee!.Department!.Name,
                a.RateeEmployee!.DesignationId
            })
            .ToListAsync(ct);

        if (departmentId is int dep)
            ratees = ratees.Where(r => r.DepartmentId == dep).ToList();

        // mapped attribute counts per designation
        var mapped = await _db.DesignationAttributeMaps
            .GroupBy(m => m.DesignationId)
            .Select(g => new { DesignationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DesignationId, x => x.Count, ct);

        // done rating counts by ratee for this rator
        var done = await _db.CompetencyRatings
            .Where(r => r.RatorEmployeeId == ratorEmployeeId && r.Status == RatingStatus.Done)
            .GroupBy(r => r.RateeEmployeeId)
            .Select(g => new { RateeId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RateeId, x => x.Count, ct);

        return ratees.Select(r =>
        {
            var need = mapped.GetValueOrDefault(r.DesignationId, 0);
            var have = done.GetValueOrDefault(r.RateeEmployeeId, 0);
            var status = need > 0 && have >= need ? "Done" : "Pending";
            return new RateeRowDto(r.RateeEmployeeId, r.Name, r.Department, status);
        }).ToList();
    }

    public async Task<IReadOnlyList<CompetencyGroup>> GetRateeAttributesAsync(int ratorEmployeeId, int rateeEmployeeId, CancellationToken ct = default)
    {
        var designationId = await _db.Employees
            .Where(e => e.Id == rateeEmployeeId).Select(e => e.DesignationId).FirstOrDefaultAsync(ct);

        var maps = await _db.DesignationAttributeMaps
            .Where(m => m.DesignationId == designationId)
            .Select(m => new
            {
                m.AttributeId,
                AttributeName = m.Attribute!.Name,
                Competency = m.Attribute!.Competency!.Name
            })
            .ToListAsync(ct);

        var existing = await _db.CompetencyRatings
            .Where(r => r.RatorEmployeeId == ratorEmployeeId && r.RateeEmployeeId == rateeEmployeeId)
            .Select(r => new { r.AttributeId, r.Rating, r.Remarks })
            .ToListAsync(ct);
        var ratingByAttr = existing.ToDictionary(x => x.AttributeId, x => (x.Rating, x.Remarks));

        return maps
            .GroupBy(m => m.Competency)
            .Select(g => new CompetencyGroup(
                g.Key,
                CompetencyColors.GetValueOrDefault(g.Key, "#2563EB"),
                g.Select(m =>
                {
                    var has = ratingByAttr.TryGetValue(m.AttributeId, out var v);
                    return new AttributeRatingRow
                    {
                        AttributeId = m.AttributeId,
                        AttributeName = m.AttributeName,
                        Rating = has ? v.Rating : 0,
                        Remarks = has ? v.Remarks : null
                    };
                }).ToList()))
            .ToList();
    }

    public async Task<Result> SaveRatingsAsync(int ratorEmployeeId, int rateeEmployeeId, IReadOnlyList<AttributeRatingRow> rows, bool submit, CancellationToken ct = default)
    {
        var ratingPeriodId = await ActiveRatingPeriodIdAsync(ct);
        if (ratingPeriodId == 0) return Result.Failure("Rating period is not active.");
        if (rows.Count == 0) return Result.Failure("No attributes to rate.");
        if (rows.Any(r => r.Rating is < 0 or > 4)) return Result.Failure("Ratings must be between 0 and 4.");

        var status = submit ? RatingStatus.Done : RatingStatus.Pending;

        var existing = await _db.CompetencyRatings
            .Where(r => r.RatingPeriodId == ratingPeriodId &&
                        r.RatorEmployeeId == ratorEmployeeId &&
                        r.RateeEmployeeId == rateeEmployeeId)
            .ToListAsync(ct);

        foreach (var row in rows)
        {
            var match = existing.FirstOrDefault(x => x.AttributeId == row.AttributeId);
            if (match is null)
            {
                _db.CompetencyRatings.Add(new CompetencyRating
                {
                    RatingPeriodId = ratingPeriodId,
                    RateeEmployeeId = rateeEmployeeId,
                    RatorEmployeeId = ratorEmployeeId,
                    AttributeId = row.AttributeId,
                    Rating = row.Rating,
                    Remarks = row.Remarks,
                    Status = status
                });
            }
            else
            {
                match.Rating = row.Rating;
                match.Remarks = row.Remarks;
                match.Status = status;
            }
        }

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
