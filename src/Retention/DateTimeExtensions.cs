namespace Larcanum.Retention;

internal static class DateTimeExtensions
{
    extension(DateTimeOffset dateTimeOffset)
    {
        /// <summary>
        /// Returns the date of the Monday of the same week as <paramref name="dateTimeOffset"/>
        /// </summary>
        public DateTimeOffset MondayOfWeek()
        {
            return dateTimeOffset.AddDays(-DaysFromMonday(dateTimeOffset.DayOfWeek));
        }
    }

    /// <summary>
    /// Determines the number of days the given <paramref name="dayOfWeek"/> is past the last Monday. (Monday being 0
    /// days away, Tuesday being 1 day away, ...)
    /// </summary>
    private static int DaysFromMonday(DayOfWeek dayOfWeek)
    {
        return ((int)dayOfWeek + 6) % 7;
    }
}
