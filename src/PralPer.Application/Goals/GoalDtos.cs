namespace PralPer.Application.Goals;

/// <summary>Editable goal row bound by the Goal Submission screen.</summary>
public sealed class GoalInput
{
    public int GoalNo { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal ProgressPercent { get; set; }
    public decimal WeightPercent { get; set; }
    public int Rating { get; set; }            // 0–4 (as per the Goal Submission prototype)

    public bool HasContent => !string.IsNullOrWhiteSpace(Title);
}

/// <summary>Read model for an existing saved goal.</summary>
public sealed record GoalView(
    int GoalNo,
    string? Title,
    string? Description,
    decimal ProgressPercent,
    decimal WeightPercent,
    int Rating);
