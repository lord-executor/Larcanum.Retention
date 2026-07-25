namespace Larcanum.Retention;

public class WeeklyInterval : IRetentionInterval
{
    private static readonly TimeSpan StepSize = TimeSpan.FromDays(7);

    public int Count { get; }

    public WeeklyInterval(int count)
    {
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
