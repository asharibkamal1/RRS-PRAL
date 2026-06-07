using PralPer.Domain.Common;

namespace PralPer.Domain.Entities;

/// <summary>A goal row in the PER report's Goal Assessment Details (pre-calculated).</summary>
public class PerReportGoalLine : BaseEntity
{
    public int PerReportId { get; set; }
    public PerReport? PerReport { get; set; }

    public int SortOrder { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal WeightPercent { get; set; }
    public decimal ProgressPercent { get; set; }
    public int Rating { get; set; }
    public int MaxRating { get; set; } = 4;
    public decimal ContributionPercent { get; set; }
}
