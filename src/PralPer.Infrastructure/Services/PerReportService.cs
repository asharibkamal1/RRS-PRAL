using Microsoft.EntityFrameworkCore;
using PralPer.Application.Reports;
using PralPer.Application.Services;
using PralPer.Domain.Enums;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Services;

/// <summary>Reads the finalized PER report snapshot from the database (no calculation).</summary>
public sealed class PerReportService : IPerReportService
{
    private readonly AppDbContext _db;

    public PerReportService(AppDbContext db) => _db = db;

    public async Task<PerReportDto?> GetReportAsync(int employeeId, CancellationToken ct = default)
    {
        var periodId = await _db.EvaluationPeriods
            .Where(p => p.Status == PeriodStatus.Active).Select(p => p.Id).FirstOrDefaultAsync(ct);
        if (periodId == 0) return null;

        var report = await _db.PerReports
            .Include(r => r.GoalLines)
            .Include(r => r.CompetencyLines)
            .Include(r => r.Employee).ThenInclude(e => e!.Department)
            .Include(r => r.Employee).ThenInclude(e => e!.Designation)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.EvaluationPeriodId == periodId && r.EmployeeId == employeeId, ct);

        if (report is null) return null;

        var e = report.Employee!;
        var periodName = await _db.EvaluationPeriods.Where(p => p.Id == periodId).Select(p => p.Name).FirstAsync(ct);

        static IReadOnlyList<string> Lines(string? s) =>
            string.IsNullOrWhiteSpace(s) ? Array.Empty<string>()
            : s.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new PerReportDto(
            e.Name,
            e.JobTitle ?? e.Designation?.Name ?? "",
            e.HrCode,
            e.Department?.Name ?? "",
            e.PayGroup ?? "",
            periodName,
            report.Approved,
            report.ManagerName,
            e.RecruitmentDate, e.LastPromotionDate, e.LastIncrementDate, e.LastIncrementBand, e.LastBonusDate, e.LastBonusBand,
            report.GoalScorePercent, report.CompetencyScorePercent, report.GoalWeightPercent, report.CompetencyWeightPercent,
            report.FinalPercent, report.Band,
            report.GoalLines.OrderBy(g => g.SortOrder)
                .Select(g => new PerReportGoalLineDto(g.Title, g.WeightPercent, g.ProgressPercent, g.Rating, g.MaxRating, g.ContributionPercent)).ToList(),
            report.CompetencyLines.OrderBy(c => c.SortOrder)
                .Select(c => new PerReportCompetencyLineDto(c.Competency, c.Score, c.MaxScore)).ToList(),
            Lines(report.Strengths), Lines(report.DevelopmentAreas),
            report.OverallComments, report.ApprovedBy, report.ApprovedOn,
            report.GoalsSubmittedOn, report.Evaluation360On, report.ManagerReviewOn, report.FinalApprovalOn);
    }
}
