namespace Larcanum.Retention;

public interface IRetentionInterval
{
    int Count { get; }

    DateTimeOffset Add(DateTimeOffset start);
    DateTimeOffset Subtract(DateTimeOffset start);
    DateTimeOffset AlignStart(DateTimeOffset timestamp);
    DateTimeOffset AlignEnd(DateTimeOffset timestamp);
}
