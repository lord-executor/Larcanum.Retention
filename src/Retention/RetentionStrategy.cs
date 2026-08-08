using System.Collections;

namespace Larcanum.Retention;

public class RetentionStrategy<T>
{
    private readonly List<RetentionPolicy> _policies;
    private readonly RetentionStrategyOptions _options;

    public RetentionStrategy(IEnumerable<RetentionPolicy> policies, RetentionStrategyOptions? options = null)
    {
        _policies = policies.ToList();
        _options = options ?? new RetentionStrategyOptions();
    }

    public RetentionResult<T> Evaluate(IEnumerable<RetentionCandidate<T>> candidates)
    {
        var sortedCandidates = candidates.OrderByDescending(c => c.Timestamp).ToList();
        var retain = new HashSet<RetentionCandidate<T>>();

        // Anything newer than the starting point will always be retained.
        var firstInRangeIndex = sortedCandidates.Index()
            .FirstOrDefault(x => x.Item.Timestamp <= _options.StartPoint).Index;
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
