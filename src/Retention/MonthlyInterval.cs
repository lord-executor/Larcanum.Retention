namespace Larcanum.Retention;

public class MonthlyInterval : IRetentionInterval
{
    public int Count { get; }

    public MonthlyInterval(int step)
    {
        Count = step;
    }

    public override string ToString()
    {
        return $"{Count}M";
    }

    public DateTimeOffset Add(DateTimeOffset start) => start.AddMonths(Count);
    public DateTimeOffset Subtract(DateTimeOffset start) => start.AddMonths(-Count);

    public DateTimeOffset AlignStart(DateTimeOffset timestamp) => new DateTimeOffset(timestamp.Year, timestamp.Month, 1, 0, 0, 0, timestamp.Offset);
    public DateTimeOffset AlignEnd(DateTimeOffset timestamp)
    {
        var nextMonth = timestamp.AddMonths(1);
        return new DateTimeOffset(nextMonth.Year, nextMonth.Month, 1, 0, 0, 0, timestamp.Offset);
    }
}
