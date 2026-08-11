using AwesomeAssertions;

namespace Larcanum.Retention.UnitTests;

public class DailyIntervalTests
{
    [Test]
    public void Add_ReturnsCountDaysAfterStart()
    {
        var interval = new DailyInterval(5);
        var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var result = interval.Add(start);
        result.Day.Should().Be(6);
    }

    [Test]
    public void Subtract_ReturnsCountDaysBefore()
    {
        var interval = new DailyInterval(5);
        var start = new DateTimeOffset(2026, 1, 6, 0, 0, 0, TimeSpan.Zero);
        var result = interval.Subtract(start);
        result.Day.Should().Be(1);
        result.Month.Should().Be(1);
    }

    [Test]
    public void AlignStart_TruncatesTimeToMidnight()
    {
        var interval = new DailyInterval(1);
        var date = new DateTimeOffset(2026, 1, 28, 15, 42, 30, TimeSpan.Zero);
        var start = interval.AlignStart(date);

        start.Year.Should().Be(2026);
        start.Month.Should().Be(1);
        start.Day.Should().Be(28);
        start.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    [Test]
    public void AlignStart_PreservesOffset()
    {
        var interval = new DailyInterval(1);
        var offset = TimeSpan.FromHours(2);
        var date = new DateTimeOffset(2026, 1, 28, 15, 42, 30, offset);
        var start = interval.AlignStart(date);

        start.Offset.Should().Be(offset);
    }

    [Test]
    public void AlignEnd_ReturnsMidnightOfNextDay()
    {
        var interval = new DailyInterval(1);
        var date = new DateTimeOffset(2026, 1, 28, 15, 42, 30, TimeSpan.Zero);
        var end = interval.AlignEnd(date);

        end.Year.Should().Be(2026);
        end.Month.Should().Be(1);
        end.Day.Should().Be(29);
        end.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    [Test]
    public void AlignEnd_WhenLastDayOfMonth_RollsOverToNextMonth()
    {
        var interval = new DailyInterval(1);
        var date = new DateTimeOffset(2026, 1, 31, 10, 0, 0, TimeSpan.Zero);
        var end = interval.AlignEnd(date);

        end.Year.Should().Be(2026);
        end.Month.Should().Be(2);
        end.Day.Should().Be(1);
    }

    [Test]
    public void ToString_ReturnsCountWithDSuffix()
    {
        var interval = new DailyInterval(3);
        interval.ToString().Should().Be("3D");
    }
}
