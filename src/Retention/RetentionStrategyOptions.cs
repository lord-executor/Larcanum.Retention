namespace Larcanum.Retention;

/// <summary>
/// Configuration for a <see cref="RetentionStrategy{T}"/> evaluation where <see cref="StartPoint"/> defines what "now"
/// is and thus which candidates are "in the past" vs "in the future".
/// </summary>
public class RetentionStrategyOptions
{
    public DateTimeOffset StartPoint { get; init; } = DateTimeOffset.UtcNow;
    public bool AllowOverlap { get; init; } = true;
}
