namespace Larcanum.Retention;

public class RetentionStrategyOptions
{
    public DateTimeOffset StartPoint { get; init; } = DateTimeOffset.UtcNow;
    public bool AllowOverlap { get; init; } = true;
}
