namespace Larcanum.Retention;

/// <summary>
/// Determines which candidate within a bucket a <see cref="RetentionPolicy"/> keeps: the most recent
/// (<see cref="Newest"/>) or the least recent (<see cref="Oldest"/>) one.
/// </summary>
public enum RetentionAlignment
{
    Newest = 0,
    Oldest = 1
}
