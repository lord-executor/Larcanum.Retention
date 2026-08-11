namespace Larcanum.Retention;

/// <summary>
/// An <see cref="IRetentionInterval"/> spanning a calendar week. Aligns to Monday of the week, using [Mo - Su] as the
/// definition of a week.
/// </summary>
public class WeeklyInterval : IRetentionInterval
{
    private static readonly TimeSpan StepSize = TimeSpan.FromDays(7);

    public int Count { get; }

    public WeeklyInterval(int count)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);
        Count = count;
    }

    public override string ToString()
    {
        return $"{Count}W";
    }

    public DateTimeOffset Add(DateTimeOffset start) => start.Add(Count * StepSize);
    public DateTimeOffset Subtract(DateTimeOffset start) => start.Add(-Count * StepSize);
    public DateTimeOffset AlignStart(DateTimeOffset timestamp) => new DateTimeOffset(timestamp.MondayOfWeek().Date, timestamp.Offset);
    public DateTimeOffset AlignEnd(DateTimeOffset timestamp) => new DateTimeOffset(timestamp.Add(StepSize).MondayOfWeek().Date, timestamp.Offset);
}
