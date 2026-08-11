using AwesomeAssertions;

namespace Larcanum.Retention.UnitTests;

public class WeeklyIntervalTests
{
    [Test]
    public void AlignStart_ReturnsMonday()
    {
        var interval = new WeeklyInterval(1);
        // 2026-01-28 is Wednesday
        var date = new DateTimeOffset(2026, 1, 28, 10, 0, 0, TimeSpan.Zero);
        var start = interval.AlignStart(date);

        start.DayOfWeek.Should().Be(DayOfWeek.Monday);
        start.Year.Should().Be(2026);
        start.Month.Should().Be(1);
        start.Day.Should().Be(26);
    }

    [Test]
    public void AlignStart_WhenAlreadyMonday_ReturnsSameDay()
    {
        var interval = new WeeklyInterval(1);
        var date = new DateTimeOffset(2026, 1, 26, 10, 0, 0, TimeSpan.Zero);
        var start = interval.AlignStart(date);

        start.Year.Should().Be(2026);
        start.Month.Should().Be(1);
        start.Day.Should().Be(26);
        start.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    [Test]
    public void AlignStart_PreservesOffset()
    {
        var interval = new WeeklyInterval(1);
        var offset = TimeSpan.FromHours(-5);
        var date = new DateTimeOffset(2026, 1, 28, 10, 0, 0, offset);
        var start = interval.AlignStart(date);

        start.Offset.Should().Be(offset);
    }

    [Test]
    public void AlignEnd_ReturnsMondayNextWeek()
    {
        var interval = new WeeklyInterval(1);
        // 2026-01-28 is Wednesday
        var date = new DateTimeOffset(2026, 1, 28, 10, 0, 0, TimeSpan.Zero);
        var end = interval.AlignEnd(date);

        end.DayOfWeek.Should().Be(DayOfWeek.Monday);
        end.Year.Should().Be(2026);
        end.Month.Should().Be(2);
        end.Day.Should().Be(2);
    }

    [Test]
    public void Add_WithCountTwo_ReturnsDayTwoWeeksAfterStart()
    {
        var interval = new WeeklyInterval(2);
        // 2026-01-28 is Wednesday
        var date = new DateTimeOffset(2026, 1, 28, 10, 0, 0, TimeSpan.Zero);
        var next = interval.Add(date);

        next.DayOfWeek.Should().Be(DayOfWeek.Wednesday);
        next.Year.Should().Be(2026);
        next.Month.Should().Be(2);
        next.Day.Should().Be(11);
    }

    [Test]
    public void Subtract_WithCountTwo_ReturnsDayTwoWeeksBeforeStart()
    {
        var interval = new WeeklyInterval(2);
        // 2026-01-28 is Wednesday
        var date = new DateTimeOffset(2026, 1, 28, 10, 0, 0, TimeSpan.Zero);
        var next = interval.Subtract(date);

        next.DayOfWeek.Should().Be(DayOfWeek.Wednesday);
        next.Year.Should().Be(2026);
        next.Month.Should().Be(1);
        next.Day.Should().Be(14);
    }

    [Test]
    public void ToString_ReturnsCountWithWSuffix()
    {
        var interval = new WeeklyInterval(2);
        interval.ToString().Should().Be("2W");
    }
}
