namespace Larcanum.Retention;

/// <summary>
/// An <see cref="IRetentionInterval"/> spanning a calendar year. Aligns to January 1st of its containing year.
/// </summary>
public class YearlyInterval : IRetentionInterval
{
    public int Count { get; }

    public YearlyInterval(int count)
    {
        Count = count;
    }

    public override string ToString()
    {
        return $"{Count}Y";
    }

    public DateTimeOffset Add(DateTimeOffset start) => start.AddYears(Count);
    public DateTimeOffset Subtract(DateTimeOffset start) => start.AddYears(-Count);
    public DateTimeOffset AlignStart(DateTimeOffset timestamp) => new DateTimeOffset(timestamp.Year, 1, 1, 0, 0, 0, timestamp.Offset);
    public DateTimeOffset AlignEnd(DateTimeOffset timestamp) => new DateTimeOffset(timestamp.Year + 1, 1, 1, 0, 0, 0, timestamp.Offset);
}
