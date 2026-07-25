using AwesomeAssertions;

namespace Larcanum.Retention.UnitTests;

public class IntervalTests
{
    [Test]
    public void WeeklyInterval_GetIntervalStart_ReturnsMonday()
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
    public void WeeklyInterval_GetIntervalEnd_ReturnsMondayNextWeek()
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
    public void BiWeeklyInterval_Add_ReturnsDayTwoWeeksAfterStart()
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
    public void BiWeeklyInterval_Subtract_ReturnsDayTwoWeeksBeforeStart()
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
    public void MonthlyInterval_GetIntervalStart_ReturnsFirstOfMonth()
    {
        var interval = new MonthlyInterval(1);
        var date = new DateTimeOffset(2026, 1, 28, 10, 0, 0, TimeSpan.Zero);
        var start = interval.AlignStart(date);

        start.Day.Should().Be(1);
        start.Month.Should().Be(1);
    }

    [Test]
    public void DailyInterval_AddIntervals_Works()
    {
        var interval = new DailyInterval(5);
        var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var result = interval.Add(start);
        result.Day.Should().Be(6);
    }
}
