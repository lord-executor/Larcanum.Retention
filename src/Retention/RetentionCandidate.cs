namespace Larcanum.Retention;

public class RetentionCandidate<T>
{
    public T Item { get; }
    public DateTimeOffset Timestamp { get; }
    public HashSet<RetentionPolicy> Policies { get; } = new();

    public RetentionCandidate(T item, DateTimeOffset timestamp)
    {
        Item = item;
        Timestamp = timestamp;
    }
}
