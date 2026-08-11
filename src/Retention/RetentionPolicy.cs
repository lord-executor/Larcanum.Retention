using System.Globalization;
using System.Text.RegularExpressions;

namespace Larcanum.Retention;

/// <summary>
/// A single GFS-style retention rule: within each <see cref="PeriodInterval"/>-sized bucket of the evaluated
/// timeline, keep one candidate per <see cref="KeepInterval"/>-sized slot, choosing the slot's newest or oldest
/// candidate per <see cref="Alignment"/>. Can be expressed as a compact string, e.g. <c>"1W:1D:N"</c> means
/// "for the most recent week, keep the newest candidate of each day". Multiple policies are combined by a
/// <see cref="RetentionStrategy{T}"/> to build a full rotation scheme (e.g. daily for a week, weekly for a
/// month, monthly for a year).
/// </summary>
public partial class RetentionPolicy
{
    [GeneratedRegex(@"^(\d+)(D|W|M|Y):(\d+)(D|W|M|Y):(N|O)$")]
    private static partial Regex PolicyExp { get; }

    public IRetentionInterval PeriodInterval { get; }
    public IRetentionInterval KeepInterval { get; }
    public RetentionAlignment Alignment { get; }

    public RetentionPolicy(IRetentionInterval periodInterval, IRetentionInterval keepInterval, RetentionAlignment alignment = RetentionAlignment.Newest)
    {
        PeriodInterval = periodInterval;
        KeepInterval = keepInterval;
        Alignment = alignment;
    }

    public override string ToString()
    {
        var alignment = Alignment == RetentionAlignment.Newest ? "N" : "O";
        return $"{PeriodInterval}:{KeepInterval}:{alignment}";
    }

    public static IList<RetentionPolicy> Parse(string syntax)
    {
        return string.IsNullOrWhiteSpace(syntax)
            ? []
            : syntax.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(ParseSingle).ToList();
    }

    private static RetentionPolicy ParseSingle(string policyStr)
    {
        // Example: 1W:1D:N
        var match = PolicyExp.Match(policyStr);
        if (!match.Success)
        {
            throw new ArgumentException($"Invalid policy expression: {policyStr}");
        }

        var period = CreateInterval(match.Groups[1].Value, match.Groups[2].Value);
        var keep = CreateInterval(match.Groups[3].Value, match.Groups[4].Value);

        var alignment = match.Groups[5].Value switch
        {
            "N" => RetentionAlignment.Newest,
            "O" => RetentionAlignment.Oldest,
            _ => throw new ArgumentException($"Invalid alignment: {match.Groups[5].Value}")
        };

        return new RetentionPolicy(period, keep, alignment);
    }

    private static IRetentionInterval CreateInterval(string count, string modifier)
    {
        int stepSize = int.Parse(count, CultureInfo.InvariantCulture);

        return modifier switch
        {
            "D" => new DailyInterval(stepSize),
            "W" => new WeeklyInterval(stepSize),
            "M" => new MonthlyInterval(stepSize),
            "Y" => new YearlyInterval(stepSize),
            _ => throw new ArgumentException($"Invalid interval modifier: {modifier}")
        };
    }
}
