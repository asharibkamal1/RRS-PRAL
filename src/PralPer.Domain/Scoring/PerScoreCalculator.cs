namespace PralPer.Domain.Scoring;

/// <summary>
/// Pure, side-effect-free PER scoring (1–10 basis), per the confirmed business rules:
///   Goal  = Σ ( (WeightPercent / 100) × Rating )   over the employee's 3–5 goals
///   Peer  = Σ ratings / count of the ratee's designation-mapped attributes
///   Final = Goal × 0.70 + Peer × 0.30
/// </summary>
public static class PerScoreCalculator
{
    public const decimal GoalWeight = 0.70m;
    public const decimal PeerWeight = 0.30m;

    /// <summary>Weighted goal score on a 0–10 scale. Weights are percentages summing to 100.</summary>
    public static decimal GoalScore(IEnumerable<GoalScoreInput> goals)
    {
        ArgumentNullException.ThrowIfNull(goals);
        return goals.Sum(g => (g.WeightPercent / 100m) * g.Rating);
    }

    /// <summary>Average peer rating on a 0–10 scale across the ratee's mapped attributes.</summary>
    public static decimal PeerScore(IReadOnlyCollection<int> attributeRatings)
    {
        ArgumentNullException.ThrowIfNull(attributeRatings);
        if (attributeRatings.Count == 0) return 0m;
        return (decimal)attributeRatings.Sum() / attributeRatings.Count;
    }

    /// <summary>Final PER score = 70% goals + 30% peer.</summary>
    public static decimal FinalScore(decimal goalScore, decimal peerScore)
        => Math.Round(goalScore * GoalWeight + peerScore * PeerWeight, 2, MidpointRounding.AwayFromZero);

    /// <summary>Colour band for a 1–10 score.</summary>
    public static ScoreBand Band(decimal score)
        => score >= 8.0m ? ScoreBand.High
         : score >= 6.0m ? ScoreBand.Mid
         : ScoreBand.Low;
}

/// <summary>Input tuple for a single goal's contribution to the goal score.</summary>
public readonly record struct GoalScoreInput(decimal WeightPercent, int Rating);
