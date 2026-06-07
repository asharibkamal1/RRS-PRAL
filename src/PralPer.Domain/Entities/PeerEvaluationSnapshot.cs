using PralPer.Domain.Common;

namespace PralPer.Domain.Entities;

/// <summary>
/// A read-only snapshot of one peer (360°) evaluation a rater performed on a ratee, used by the
/// manager dashboard popup and the read-only 360° detail view. Pre-calculated / dummy-seeded.
/// </summary>
public class PeerEvaluationSnapshot : BaseEntity
{
    public int RaterId { get; set; }
    public ManagerRater? Rater { get; set; }

    public int SortOrder { get; set; }
    public string RateeName { get; set; } = string.Empty;
    public string RateeDepartment { get; set; } = string.Empty;
    public string RateeJobTitle { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";          // Completed / Pending

    public string PeriodName { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }               // e.g. 6.0 (out of 10, per prototype)
    public int CompetenciesRated { get; set; }
    public decimal AssessmentScorePercent { get; set; }      // e.g. 60.0

    public ICollection<PeerEvaluationLine> Lines { get; set; } = new List<PeerEvaluationLine>();
}

/// <summary>One attribute line in a peer-evaluation snapshot (score out of 4).</summary>
public class PeerEvaluationLine : BaseEntity
{
    public int SnapshotId { get; set; }
    public PeerEvaluationSnapshot? Snapshot { get; set; }

    public int SortOrder { get; set; }
    public string Competency { get; set; } = string.Empty;
    public string AttributeName { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public int Score { get; set; }
    public int MaxScore { get; set; } = 4;
}
