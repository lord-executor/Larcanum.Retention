namespace Larcanum.Retention;

/// <summary>
/// Represents a recurring calendar interval with a step size, used for both <see cref="RetentionPolicy.PeriodInterval"/>
/// that defines the "bucket size" of a retention rule and <see cref="RetentionPolicy.KeepInterval"/> which represents
/// the "keep every N of" cadence within a bucket. Intervals are always aligned to a fixed start/end point.
/// </summary>
public interface IRetentionInterval
{
    /// <summary>
    /// The number N of steps of this interval type that fully defines the duration of the interval.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Adds the interval duration to the given <paramref name="start"/> value. While the assumption is generally that
    /// the given <paramref name="start"/> is already aligned to the interval borders, this fact is not enforced.
    /// </summary>
    DateTimeOffset Add(DateTimeOffset start);
    /// <summary>
    /// Subtracts the interval duration from the given <paramref name="start"/> value. While the assumption is generally that
    /// the given <paramref name="start"/> is already aligned to the interval borders, this fact is not enforced.
    /// </summary>
    DateTimeOffset Subtract(DateTimeOffset start);
    /// <summary>
    /// Returns the closest aligned starting point of this interval from the <paramref name="timestamp"/>.
    /// </summary>
    DateTimeOffset AlignStart(DateTimeOffset timestamp);
    /// <summary>
    /// Returns the closet aligned end point of this interval from the <paramref name="timestamp"/>.
    /// </summary>
    DateTimeOffset AlignEnd(DateTimeOffset timestamp);
}
