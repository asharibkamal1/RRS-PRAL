using PralPer.Domain.Scoring;
using Xunit;

namespace PralPer.UnitTests;

public class PerScaleTests
{
    [Theory]
    [InlineData(95, 4)]
    [InlineData(90, 4)]
    [InlineData(89.99, 3)]
    [InlineData(80, 3)]
    [InlineData(75, 2)]
    [InlineData(60, 2)]
    [InlineData(59, 1)]
    [InlineData(40, 1)]
    [InlineData(39, 0)]
    [InlineData(0, 0)]
    public void Bucket_MapsFinalPercentToCategory(decimal final, int expected)
        => Assert.Equal(expected, PerScale.Bucket(final));

    [Theory]
    [InlineData(95, "Outstanding")]
    [InlineData(85, "Excellent")]
    [InlineData(70, "Meets Expectations")]
    [InlineData(45, "Needs Improvement")]
    [InlineData(20, "Below Expectations")]
    public void Band_MatchesBucket(decimal final, string expected)
        => Assert.Equal(expected, PerScale.Band(final));

    [Theory]
    [InlineData(25, 0.25)]
    [InlineData(100, 1.0)]
    [InlineData(0, 0)]
    public void ToFraction_DividesByHundred(decimal percent, decimal expected)
        => Assert.Equal(expected, PerScale.ToFraction(percent));

    [Theory]
    [InlineData(0.25, 25)]
    [InlineData(1.0, 100)]
    public void ToPercent_IsInverseOfToFraction(decimal fraction, decimal expected)
        => Assert.Equal(expected, PerScale.ToPercent(fraction));

    [Fact]
    public void FinalScore_AppliesSeventyThirtyWeighting()
    {
        // 80 * 0.70 + 60 * 0.30 = 56 + 18 = 74
        Assert.Equal(74m, PerScale.FinalScore(80m, 60m));
    }

    [Fact]
    public void Weights_SumToOne() => Assert.Equal(1.0m, PerScale.GoalWeight + PerScale.CompetencyWeight);

    [Fact]
    public void RelativeTime_FormatsRanges()
    {
        var now = new DateTime(2026, 6, 7, 12, 0, 0, DateTimeKind.Utc);
        Assert.Equal("just now", PerScale.RelativeTime(now.AddSeconds(-10), now));
        Assert.Equal("5 minutes ago", PerScale.RelativeTime(now.AddMinutes(-5), now));
        Assert.Equal("3 hours ago", PerScale.RelativeTime(now.AddHours(-3), now));
        Assert.Equal("2 days ago", PerScale.RelativeTime(now.AddDays(-2), now));
    }
}
