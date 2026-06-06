using PralPer.Domain.Scoring;
using Xunit;

namespace PralPer.UnitTests;

public class PerScoreCalculatorTests
{
    [Fact]
    public void GoalScore_WeightsSumTo100_ProducesWeightedAverageOnTenScale()
    {
        var goals = new[]
        {
            new GoalScoreInput(25, 8),
            new GoalScoreInput(20, 9),
            new GoalScoreInput(15, 7),
            new GoalScoreInput(30, 7),
            new GoalScoreInput(10, 8),
        };

        // 0.25*8 + 0.20*9 + 0.15*7 + 0.30*7 + 0.10*8 = 2 + 1.8 + 1.05 + 2.1 + 0.8 = 7.75
        Assert.Equal(7.75m, PerScoreCalculator.GoalScore(goals));
    }

    [Fact]
    public void PeerScore_IsAverageOfRatings()
    {
        var ratings = new[] { 8, 8, 8, 8, 8, 8, 8, 8, 8, 8 };
        Assert.Equal(8m, PerScoreCalculator.PeerScore(ratings));
    }

    [Fact]
    public void PeerScore_EmptyRatings_IsZero()
        => Assert.Equal(0m, PerScoreCalculator.PeerScore(Array.Empty<int>()));

    [Fact]
    public void FinalScore_Applies70_30Split()
    {
        // 7.75*0.70 + 8*0.30 = 5.425 + 2.4 = 7.825 -> rounded 7.83
        Assert.Equal(7.83m, PerScoreCalculator.FinalScore(7.75m, 8m));
    }

    [Theory]
    [InlineData(8.0, ScoreBand.High)]
    [InlineData(7.99, ScoreBand.Mid)]
    [InlineData(6.0, ScoreBand.Mid)]
    [InlineData(5.99, ScoreBand.Low)]
    public void Band_UsesCrfThresholds(double score, ScoreBand expected)
        => Assert.Equal(expected, PerScoreCalculator.Band((decimal)score));
}
