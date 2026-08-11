using AwesomeAssertions;

namespace Larcanum.Retention.UnitTests;

public class MonthlyIntervalTests
{
    [Test]
    public void AlignStart_ReturnsFirstOfMonth()
    {
        var interval = new MonthlyInterval(1);
        var date = new DateTimeOffset(2026, 1, 28, 10, 0, 0, TimeSpan.Zero);
        var start = interval.AlignStart(date);

        start.Day.Should().Be(1);
        start.Month.Should().Be(1);
    }

    [Test]
    public void AlignStart_ResetsTimeToMidnight()
    {
        var interval = new MonthlyInterval(1);
        var date = new DateTimeOffset(2026, 1, 28, 15, 42, 30, TimeSpan.Zero);
        var start = interval.AlignStart(date);

        start.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    [Test]
    public void AlignEnd_ReturnsFirstOfNextMonth()
    {
        var interval = new MonthlyInterval(1);
        var date = new DateTimeOffset(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);
        var end = interval.AlignEnd(date);

        end.Year.Should().Be(2026);
        end.Month.Should().Be(2);
        end.Day.Should().Be(1);
    }

    [Test]
    public void AlignEnd_WhenDecember_RollsOverToNextYear()
    {
        var interval = new MonthlyInterval(1);
        var date = new DateTimeOffset(2026, 12, 15, 10, 0, 0, TimeSpan.Zero);
        var end = interval.AlignEnd(date);

        end.Year.Should().Be(2027);
        end.Month.Should().Be(1);
        end.Day.Should().Be(1);
    }

    [Test]
    public void Add_AddsCountMonths()
    {
        var interval = new MonthlyInterval(3);
        var start = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
        var result = interval.Add(start);

        result.Year.Should().Be(2026);
        result.Month.Should().Be(4);
        result.Day.Should().Be(15);
    }

    [Test]
    public void Add_WhenTargetMonthIsShorter_ClampsToLastDay()
    {
        var interval = new MonthlyInterval(1);
        var start = new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero);
        var result = interval.Add(start);

        result.Year.Should().Be(2026);
        result.Month.Should().Be(2);
        result.Day.Should().Be(28);
    }

    [Test]
    public void Subtract_SubtractsCountMonths()
    {
        var interval = new MonthlyInterval(2);
        var start = new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero);
        var result = interval.Subtract(start);

        result.Year.Should().Be(2026);
        result.Month.Should().Be(1);
        result.Day.Should().Be(15);
    }

    [Test]
    public void Subtract_WhenCrossingYearBoundary_RollsOverToPreviousYear()
    {
        var interval = new MonthlyInterval(2);
        var start = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
        var result = interval.Subtract(start);

        result.Year.Should().Be(2025);
        result.Month.Should().Be(11);
        result.Day.Should().Be(15);
    }

    [Test]
    public void ToString_ReturnsCountWithMSuffix()
    {
        var interval = new MonthlyInterval(6);
        interval.ToString().Should().Be("6M");
    }
}
