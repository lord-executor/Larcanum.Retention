namespace Larcanum.Retention;

public class RetentionResult<T>
{
    public IReadOnlyList<RetentionCandidate<T>> Retain { get; }
    public IReadOnlyList<RetentionCandidate<T>> Prune { get; }

    public RetentionResult(IEnumerable<RetentionCandidate<T>> retain, IEnumerable<RetentionCandidate<T>> prune)
    {
        Retain = retain.ToList();
        Prune = prune.ToList();
    }
}
