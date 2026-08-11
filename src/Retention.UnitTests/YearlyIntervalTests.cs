using AwesomeAssertions;

namespace Larcanum.Retention.UnitTests;

public class YearlyIntervalTests
{
    [Test]
    public void AlignStart_ReturnsJanuaryFirst()
    {
        var interval = new YearlyInterval(1);
        var date = new DateTimeOffset(2026, 7, 15, 10, 0, 0, TimeSpan.Zero);
        var start = interval.AlignStart(date);

        start.Year.Should().Be(2026);
        start.Month.Should().Be(1);
        start.Day.Should().Be(1);
        start.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    [Test]
    public void AlignEnd_ReturnsJanuaryFirstOfNextYear()
    {
        var interval = new YearlyInterval(1);
        var date = new DateTimeOffset(2026, 7, 15, 10, 0, 0, TimeSpan.Zero);
        var end = interval.AlignEnd(date);

        end.Year.Should().Be(2027);
        end.Month.Should().Be(1);
        end.Day.Should().Be(1);
    }

    [Test]
    public void Add_AddsCountYears()
    {
        var interval = new YearlyInterval(2);
        var start = new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero);
        var result = interval.Add(start);

        result.Year.Should().Be(2028);
        result.Month.Should().Be(3);
        result.Day.Should().Be(15);
    }

    [Test]
    public void Add_WhenLeapDay_ClampsToFeb28()
    {
        var interval = new YearlyInterval(1);
        var start = new DateTimeOffset(2028, 2, 29, 0, 0, 0, TimeSpan.Zero);
        var result = interval.Add(start);

        result.Year.Should().Be(2029);
        result.Month.Should().Be(2);
        result.Day.Should().Be(28);
    }

    [Test]
    public void Subtract_SubtractsCountYears()
    {
        var interval = new YearlyInterval(3);
        var start = new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero);
        var result = interval.Subtract(start);

        result.Year.Should().Be(2023);
        result.Month.Should().Be(3);
        result.Day.Should().Be(15);
    }

    [Test]
    public void ToString_ReturnsCountWithYSuffix()
    {
        var interval = new YearlyInterval(1);
        interval.ToString().Should().Be("1Y");
    }
}
