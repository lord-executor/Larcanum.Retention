using System.Globalization;

using AwesomeAssertions;

namespace Larcanum.Retention.UnitTests;

public class RetentionStrategyUseCaseTests
{
    /// <summary>
    /// Wednesday 2026-01-28 12:00:00
    /// </summary>
    private static readonly DateTimeOffset StartPoint = new(2026, 1, 28, 12, 0, 0, TimeSpan.Zero);

    [Test]
    [MethodDataSource(nameof(GenerateTestScenarios))]
    public void Evaluate_PoliciesAgainstCandidates_PrunesExpectedItems(string policyDef, List<RetentionCandidate<SelfAwareItem>> candidates)
    {
        var policies = RetentionPolicy.Parse(policyDef);
        var strategy = new RetentionStrategy<SelfAwareItem>(policies, new RetentionStrategyOptions { StartPoint = StartPoint, AllowOverlap = false });

        var result = strategy.Evaluate(candidates);

        result.Retain.Should().OnlyContain(c => c.Item.IsRetained);
        result.Prune.Should().OnlyContain(c => !c.Item.IsRetained);
        (result.Retain.Count + result.Prune.Count).Should().Be(candidates.Count);
    }

    [Test]
    public void Evolution_WithDailyRetentionCandidates_ProducesExpectedResult()
    {
        var policies = RetentionPolicy.Parse("7D:1D:N,4W:1W:N,12M:1M:N");
        List<RetentionCandidate<string>> candidates = [];

        // Starts at 2024-11-04
        var currentDate = StartPoint.AddDays(-450);
        for (var i = 0; i < 450; i++)
        {
            currentDate = currentDate.AddDays(1);
            candidates.Add(new RetentionCandidate<string>(DateOnly.FromDateTime(currentDate.Date).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), currentDate.AddHours(-2)));
            var strategy = new RetentionStrategy<string>(policies, new RetentionStrategyOptions { StartPoint = currentDate, AllowOverlap = true });
            var result = strategy.Evaluate(candidates);
            // We have to create _new_ candidates since the evaluated candidates already contain policy information
            // that would interfere with the next iteration
            candidates = result.Retain
                .Select(c => new RetentionCandidate<string>(c.Item, c.Timestamp))
                .ToList();
        }

        currentDate.Should().Be(StartPoint);
        candidates
            .Select(c => c.Item)
            .OrderDescending()
            .Should().BeEquivalentTo([
                // dailies
                "2026-01-28", "2026-01-27", "2026-01-26", "2026-01-25", "2026-01-24", "2026-01-23", "2026-01-22", "2026-01-21",
                // weeklies
                "2026-01-18", "2026-01-11", "2026-01-04",
                // monthlies
                "2025-12-31", "2025-11-30", "2025-10-31", "2025-09-30", "2025-08-31", "2025-07-31",
                "2025-06-30", "2025-05-31", "2025-04-30", "2025-03-31", "2025-02-28", "2025-01-31"
            ]);
    }

    public static IEnumerable<object?[]> GenerateTestScenarios()
    {
        yield return
        [
            "1W:1D:N,4W:1W:N,12M:1M:N", CandidateFactory([
                (7, true),
                (28, true),
                (360, true)
            ])
        ];
        yield return
        [
            "7D:2D:N,4W:2W:N", CandidateFactory(Enumerable.Range(1, 45)
            .Select(x => (x, x switch
            {
                // the newest one is always the newest in the first 7D:2D segment
                1 => true,
                // the 2nd, 4th and 6th are the newest in their respective 7D:2D segments
                var n when n < 8 && n % 2 == 0 => true,
                // the last 3 Sundays are kept since 4W + the CURRENT week is actually 3 2W segments
                3 or 10 or 24 => true,
                _ => false
            })))
        ];
    }

    private static List<RetentionCandidate<SelfAwareItem>> CandidateFactory(IEnumerable<(int DaysAgo, bool IsRetained)> data)
    {
        return data.Select(x => new SelfAwareItem(StartPoint.AddDays(-x.DaysAgo), x.IsRetained).ToCandidate()).ToList();
    }

    public class SelfAwareItem
    {
        public DateTimeOffset Timestamp { get; }
        public bool IsRetained { get; }

        public SelfAwareItem(DateTimeOffset timestamp, bool isRetained)
        {
            Timestamp = timestamp;
            IsRetained = isRetained;
        }

        public RetentionCandidate<SelfAwareItem> ToCandidate() => new(this, Timestamp);

        public override string ToString() => $"[{Timestamp}] {IsRetained}";
    }
}
