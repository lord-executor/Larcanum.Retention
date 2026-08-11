namespace Larcanum.Retention;

/// <summary>
/// The outcome of evaluating a <see cref="RetentionStrategy{T}"/> against a set of candidates: the ones to
/// <see cref="Retain"/> because at least one policy chose to keep them, and the ones to <see cref="Prune"/>
/// because no policy did.
/// </summary>
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
