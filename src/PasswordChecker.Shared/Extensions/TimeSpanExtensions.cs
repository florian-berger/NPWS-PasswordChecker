namespace PasswordChecker.Shared.Extensions
{
    /// <summary>
    ///     Class providing extension methods for <see cref="TimeSpan" />s
    /// </summary>
    public static class TimeSpanExtensions
    {
        public static string ToReadableFullText(this TimeSpan input, string textDays, string textHours, string textMinutes, string textSeconds)
        {
            if (input.Days > 0)
            {
                return $"{input.Days} {textDays}, " +
                       $"{input.Hours} {textHours}, " +
                       $"{input.Minutes} {textMinutes}, " +
                       $"{input.Seconds} {textSeconds}";
            }

            if (input.Hours > 0)
            {
                return $"{input.Hours} {textHours}, " +
                       $"{input.Minutes} {textMinutes}, " +
                       $"{input.Seconds} {textSeconds}";
            }

            if (input.Minutes > 0)
            {
                return $"{input.Minutes} {textMinutes}, " +
                       $"{input.Seconds} {textSeconds}";
            }

            
            return $"{input.Seconds} {textSeconds}";
        }
    }
}
