namespace PralPer.Domain.Scoring;

/// <summary>Colour band for a PER score (1–10 basis).</summary>
public enum ScoreBand
{
    Low = 0,   // &lt; 6.0  -> red
    Mid = 1,   // &gt;= 6.0 -> amber
    High = 2   // &gt;= 8.0 -> green
}
