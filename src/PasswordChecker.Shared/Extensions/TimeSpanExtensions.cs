namespace PasswordChecker.Shared.Extensions
{
    /// <summary>
    ///     Class providing extension methods for <see cref="TimeSpan" />s
    /// </summary>
    public static class TimeSpanExtensions
    {
        public static string ToReadableFullText(this TimeSpan input)
        {
            string durationString;
            if (input.Days > 0)
            {
                return $"{input.Days} days, {input.Hours} hours, {input.Minutes} minutes, {input.Seconds} seconds";
            }

            if (input.Hours > 0)
            {
                return $"{input.Hours} hours, {input.Minutes} minutes, {input.Seconds} seconds";
            }

            if (input.Minutes > 0)
            {
                return $"{input.Minutes} minutes, {input.Seconds} seconds";
            }

            
            return $"{input.Seconds} seconds";
        }
    }
}
