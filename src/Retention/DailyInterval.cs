namespace Larcanum.Retention;

/// <summary>
/// An <see cref="IRetentionInterval"/> spanning a calendar day. Aligns to midnight of its containing day.
/// </summary>
public class DailyInterval : IRetentionInterval
{
    public int Count { get; }

    public DailyInterval(int step)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(step, 1);
        Count = step;
    }

    public override string ToString()
    {
        return $"{Count}D";
    }

    public DateTimeOffset Add(DateTimeOffset start) => start.AddDays(Count);
    public DateTimeOffset Subtract(DateTimeOffset start) => start.AddDays(-Count);
    public DateTimeOffset AlignStart(DateTimeOffset timestamp) => new DateTimeOffset(timestamp.Date, timestamp.Offset);
    public DateTimeOffset AlignEnd(DateTimeOffset timestamp) => new DateTimeOffset(timestamp.AddDays(1).Date, timestamp.Offset);
}
