namespace PralPer.Application.Abstractions;

/// <summary>Abstracts the system clock for testability and consistent UTC handling.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
    DateTime Today { get; }
}
