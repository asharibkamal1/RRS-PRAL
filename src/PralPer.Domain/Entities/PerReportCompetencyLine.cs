using PralPer.Domain.Common;

namespace PralPer.Domain.Entities;

/// <summary>A competency row in the PER report's Competency Assessment (pre-calculated).</summary>
public class PerReportCompetencyLine : BaseEntity
{
    public int PerReportId { get; set; }
    public PerReport? PerReport { get; set; }

    public int SortOrder { get; set; }
    public string Competency { get; set; } = string.Empty;
    public int Score { get; set; }
    public int MaxScore { get; set; } = 4;
}
