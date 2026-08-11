namespace Larcanum.Retention;

/// <summary>
/// Wraps an item of type <typeparamref name="T"/> (e.g. a backup or artifact) together with the timestamp it is
/// evaluated by, and tracks which <see cref="RetentionPolicy"/> instances have chosen to keep it once a
/// <see cref="RetentionStrategy{T}"/> has been evaluated.
/// </summary>
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
