using AwesomeAssertions;

namespace Larcanum.Retention.UnitTests;

public class RetentionPolicyParseTests
{
    [Test]
    public void Parse_DefaultPolicy_ReturnsExpectedPolicies()
    {
        string syntax = "1W:1D:N,4W:1W:N,12M:1M:N";
        var policies = RetentionPolicy.Parse(syntax).ToList();

        policies.Should().HaveCount(3);

        policies[0].PeriodInterval.Should().BeOfType<WeeklyInterval>()
            .Which.Count.Should().Be(1);
        policies[0].KeepInterval.Should().BeOfType<DailyInterval>()
            .Which.Count.Should().Be(1);
        policies[0].Alignment.Should().Be(RetentionAlignment.Newest);

        policies[1].PeriodInterval.Should().BeOfType<WeeklyInterval>()
            .Which.Count.Should().Be(4);
        policies[1].KeepInterval.Should().BeOfType<WeeklyInterval>()
            .Which.Count.Should().Be(1);
        policies[1].Alignment.Should().Be(RetentionAlignment.Newest);

        policies[2].PeriodInterval.Should().BeOfType<MonthlyInterval>()
            .Which.Count.Should().Be(12);
        policies[2].KeepInterval.Should().BeOfType<MonthlyInterval>()
            .Which.Count.Should().Be(1);
        policies[2].Alignment.Should().Be(RetentionAlignment.Newest);
    }

    [Test]
    public void Parse_WithAlignment_ReturnsExpectedAlignment()
    {
        var policies = RetentionPolicy.Parse("42M:101W:O").ToList();
        policies.Should().ContainSingle();
        policies[0].PeriodInterval.Should().BeOfType<MonthlyInterval>()
            .Which.Count.Should().Be(42);
        policies[0].KeepInterval.Should().BeOfType<WeeklyInterval>()
            .Which.Count.Should().Be(101);
        policies[0].Alignment.Should().Be(RetentionAlignment.Oldest);
    }

    [Test]
    public void Parse_InvalidFormat_ThrowsArgumentException()
    {
        Action act1 = () => RetentionPolicy.Parse("1W:1D:N:Extra");
        act1.Should().Throw<ArgumentException>();

        Action act2 = () => RetentionPolicy.Parse("1W");
        act2.Should().Throw<ArgumentException>();

        Action act3 = () => RetentionPolicy.Parse("W1:D1");
        act3.Should().Throw<ArgumentException>();
    }
}
