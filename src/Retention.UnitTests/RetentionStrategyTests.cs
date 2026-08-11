using AwesomeAssertions;

namespace Larcanum.Retention.UnitTests;

public class RetentionStrategyTests
{
    /// <summary>
    /// Wednesday 2026-01-28 12:00:00
    /// </summary>
    private static readonly DateTimeOffset StartPoint = new(2026, 1, 28, 12, 0, 0, TimeSpan.Zero);

    [Test]
    public void Evaluate_RetainsItemsNewerThanStartPoint()
    {
        var policies = new List<RetentionPolicy>();
        var strategy = new RetentionStrategy<string>(policies, new RetentionStrategyOptions { StartPoint = StartPoint });

        var candidates = new List<RetentionCandidate<string>>
        {
            new("Future", StartPoint.AddHours(1)),
            new("Past", StartPoint.AddHours(-1))
        };

        var result = strategy.Evaluate(candidates);

        result.Retain.Should().Contain(c => c.Item == "Future");
        result.Retain.Should().NotContain(c => c.Item == "Past");
    }

    [Test]
    public void Evaluate_AllCandidatesNewerThanStartPoint_RetainsAll()
    {
        // Regression test: when every candidate is newer than StartPoint (e.g. StartPoint predates
        // the whole data set), the "always retain newer than start point" rule must apply to all of
        // them rather than falling through to policy evaluation, which would only keep one per policy.
        var policies = RetentionPolicy.Parse("1Y:1D:N");
        var strategy = new RetentionStrategy<string>(policies, new RetentionStrategyOptions { StartPoint = StartPoint.AddYears(-5) });

        var candidates = Enumerable.Range(0, 10)
            .Select(i => new RetentionCandidate<string>($"Backup {i}", StartPoint.AddDays(-i)))
            .ToList();

        var result = strategy.Evaluate(candidates);

        result.Retain.Should().HaveCount(10);
        result.Prune.Should().BeEmpty();
    }

    [Test]
    public void Evaluate_DailyPolicy_RetainsOnePerDay()
    {
        // Retain 1 item per day for the last 7 days
        var policies = RetentionPolicy.Parse("7D:1D:N");
        var strategy = new RetentionStrategy<string>(policies, new RetentionStrategyOptions { StartPoint = StartPoint });

        var candidates = new List<RetentionCandidate<string>>
        {
            new("Today 1", StartPoint.AddHours(-1)),
            new("Today 2", StartPoint.AddHours(-2)),
            new("Yesterday", StartPoint.AddDays(-1).AddHours(-1)),
            new("Way back", StartPoint.AddDays(-10))
        };

        var result = strategy.Evaluate(candidates);

        // Today 1 should be retained (newest of today)
        // Yesterday should be retained
        // Way back should be pruned

        result.Retain.Should().HaveCount(2);
        result.Retain.Should().Contain(c => c.Item == "Today 1");
        result.Retain.Should().Contain(c => c.Item == "Yesterday");
        result.Prune.Should().Contain(c => c.Item == "Today 2");
        result.Prune.Should().Contain(c => c.Item == "Way back");
    }

    [Test]
    public void Evaluate_AllowOverlapFalse_TagsCandidates()
    {
        // 1st policy: retain newest in past 1 day + CURRENT day (daily segments)
        // 2nd policy: retain newest in past 1 week + CURRENT week (weekly segments)
        var policies = RetentionPolicy.Parse("1D:1D:N,1W:1W:N").ToList();
        var strategy = new RetentionStrategy<string>(policies, new RetentionStrategyOptions
        {
            StartPoint = StartPoint,
            AllowOverlap = false
        });

        var candidates = new List<RetentionCandidate<string>>
        {
            new("Day 1", StartPoint.AddDays(0)),
            new("Day 2", StartPoint.AddDays(-1)),
            new("Day 3", StartPoint.AddDays(-2))
        };

        var result = strategy.Evaluate(candidates);

        // Policy 1 should pick Day 1 (CURRENT) and Day 2 (YESTERDAY).
        // Policy 2 (1W:1W) would normally pick Day 1 for the CURRENT week (newest in week),
        // but if AllowOverlap is false, it should pick Day 3 because Day 1 and Day 2 are already tagged.

        result.Retain.Should().Contain(c => c.Item == "Day 1");
        result.Retain.Should().Contain(c => c.Item == "Day 2");
        result.Retain.Should().Contain(c => c.Item == "Day 3");

        var day1 = result.Retain.First(c => c.Item == "Day 1");
        var day2 = result.Retain.First(c => c.Item == "Day 2");
        var day3 = result.Retain.First(c => c.Item == "Day 3");

        day1.Policies.Should().Contain(policies[0]);
        day1.Policies.Should().NotContain(policies[1]);

        day2.Policies.Should().Contain(policies[0]);

        day3.Policies.Should().Contain(policies[1]);
        day3.Policies.Should().NotContain(policies[0]);
    }
}
