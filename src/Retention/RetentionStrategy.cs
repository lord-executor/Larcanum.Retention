using System.Collections;

namespace Larcanum.Retention;

/// <summary>
/// Evaluates a set of <see cref="RetentionPolicy"/> rules against a set of <see cref="RetentionCandidate{T}"/>
/// items to decide which to retain and which to prune, implementing GFS-style ("grandfather-father-son")
/// rotation. Candidates newer than <see cref="RetentionStrategyOptions.StartPoint"/> are always retained; for
/// the rest, each policy partitions the timeline into period-sized buckets and keeps one candidate per
/// keep-sized slot within each bucket. A candidate is retained overall if any policy chooses to keep it.
/// </summary>
public class RetentionStrategy<T>
{
    private readonly List<RetentionPolicy> _policies;
    private readonly RetentionStrategyOptions _options;

    public RetentionStrategy(IEnumerable<RetentionPolicy> policies, RetentionStrategyOptions? options = null)
    {
        _policies = policies.ToList();
        _options = options ?? new RetentionStrategyOptions();
    }

    /// <summary>
    /// Evaluates the retention strategy with all its policies against the set of retention <paramref name="candidates"/>
    /// and returns a <see cref="RetentionResult{T}"/> that has partitioned the candidates into the ones that should be
    /// kept and the ones that should be pruned according to these policies.
    /// </summary>
    public RetentionResult<T> Evaluate(IEnumerable<RetentionCandidate<T>> candidates)
    {
        var sortedCandidates = candidates.OrderByDescending(c => c.Timestamp).ToList();
        var retain = new HashSet<RetentionCandidate<T>>();

        // Anything newer than the starting point will always be retained.
        var firstInRangeIndexOrNotFound = sortedCandidates.FindIndex(c => c.Timestamp <= _options.StartPoint);
        var firstInRangeIndex = firstInRangeIndexOrNotFound == -1 ? sortedCandidates.Count : firstInRangeIndexOrNotFound;
        foreach (var candidate in sortedCandidates[..firstInRangeIndex])
        {
            retain.Add(candidate);
        }

        // We proceed only with candidates that are older than the starting point to make the rest of the
        // process easier.
        sortedCandidates = sortedCandidates[firstInRangeIndex..];

        foreach (var policy in _policies)
        {
            var splitGenerator = new SplitGenerator(_options.StartPoint, policy);

            foreach (var segment in BuildCandidateSegments(sortedCandidates, splitGenerator))
            {
                var filteredSegment = _options.AllowOverlap
                    ? segment
                    : segment.Where(c => c.Policies.Count == 0);

                var chosen = policy.Alignment == RetentionAlignment.Newest
                    ? filteredSegment.FirstOrDefault()
                    : filteredSegment.LastOrDefault();

                if (chosen != null)
                {
                    chosen.Policies.Add(policy);
                    retain.Add(chosen);
                }
            }
        }

        var prune = sortedCandidates.Where(c => !retain.Contains(c)).ToList();

        return new RetentionResult<T>(retain, prune);
    }

    private static IEnumerable<List<RetentionCandidate<T>>> BuildCandidateSegments(List<RetentionCandidate<T>> sortedCandidates, SplitGenerator generator)
    {
        var index = 0;
        var segment = new List<RetentionCandidate<T>>();

        foreach (var split in generator)
        {
            while (index < sortedCandidates.Count && sortedCandidates[index].Timestamp >= split)
            {
                segment.Add(sortedCandidates[index]);
                index++;
            }

            if (segment.Count > 0)
            {
                yield return segment;
                segment = new List<RetentionCandidate<T>>();
            }
        }
    }

    /// <summary>
    /// Produces the sequence of "keep interval" boundary timestamps within a single policy's period, walking
    /// backward from the period's end to its start. Consecutive boundaries delimit the slots that
    /// <see cref="BuildCandidateSegments"/> groups candidates into.
    /// </summary>
    private sealed class SplitGenerator : IEnumerable<DateTimeOffset>
    {
        private readonly RetentionPolicy _policy;

        public DateTimeOffset PeriodStart { get; }
        public DateTimeOffset PeriodEnd { get; }

        public SplitGenerator(DateTimeOffset referenceTime, RetentionPolicy policy)
        {
            _policy = policy;
            PeriodStart = _policy.PeriodInterval.Subtract(policy.PeriodInterval.AlignStart(referenceTime));
            PeriodEnd = _policy.PeriodInterval.AlignEnd(referenceTime);
        }

        public IEnumerator<DateTimeOffset> GetEnumerator()
        {
            var split = PeriodEnd;
            while (split > PeriodStart)
            {
                split = _policy.KeepInterval.Subtract(split);
                yield return split;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
