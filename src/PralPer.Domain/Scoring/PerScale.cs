namespace PralPer.Domain.Scoring;

/// <summary>
/// Central, side-effect-free PER scoring + presentation math. Keeping these rules in one place
/// (instead of scattered literals across services/pages) makes them easy to reason about and unit-test.
/// Business scoring of record (goal/peer/final) is persisted by the database; these helpers cover the
/// shared mappings the app uses to display and bucket those stored values.
/// </summary>
public static class PerScale
{
    /// <summary>Goal assessment contributes 70% of the final PER score.</summary>
    public const decimal GoalWeight = 0.70m;

    /// <summary>360° competency assessment contributes 30% of the final PER score.</summary>
    public const decimal CompetencyWeight = 0.30m;

    /// <summary>Maps a 0–100 final percentage to a 0–4 performance category.</summary>
    public static int Bucket(decimal finalPercent) => finalPercent switch
    {
        >= 90 => 4,
        >= 80 => 3,
        >= 60 => 2,
        >= 40 => 1,
        _ => 0
    };

    /// <summary>Human-readable band for a 0–100 final percentage.</summary>
    public static string Band(decimal finalPercent) => Bucket(finalPercent) switch
    {
        4 => "Outstanding",
        3 => "Excellent",
        2 => "Meets Expectations",
        1 => "Needs Improvement",
        _ => "Below Expectations"
    };

    /// <summary>Converts a whole percentage (0–100) to its stored fraction (0–1).</summary>
    public static decimal ToFraction(decimal percent) => percent / 100m;

    /// <summary>Converts a stored fraction (0–1) to a whole percentage (0–100).</summary>
    public static decimal ToPercent(decimal fraction) => fraction * 100m;

    /// <summary>Weighted final PER score from the goal and competency percentages.</summary>
    public static decimal FinalScore(decimal goalScorePercent, decimal competencyScorePercent)
        => Math.Round(goalScorePercent * GoalWeight + competencyScorePercent * CompetencyWeight, 2);

    /// <summary>Compact relative time label (e.g. "2 minutes ago") for activity feeds.</summary>
    public static string RelativeTime(DateTime utc, DateTime nowUtc)
    {
        var span = nowUtc - utc;
        if (span.TotalMinutes < 1) return "just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} minutes ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours} hours ago";
        return $"{(int)span.TotalDays} days ago";
    }
}
