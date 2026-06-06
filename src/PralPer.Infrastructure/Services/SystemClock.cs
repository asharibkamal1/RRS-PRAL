using PralPer.Application.Abstractions;

namespace PralPer.Infrastructure.Services;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    public DateTime Today => DateTime.UtcNow.Date;
}
