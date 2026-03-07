using System.Globalization;

namespace WeeklyTimetable.Converters;

public class SecondsToTimeConverter : IValueConverter
{
    /// <summary>
    /// Converts a second count into <c>mm:ss</c> timer text.
    /// </summary>
    /// <param name="value">Total seconds value.</param>
    /// <param name="targetType">Requested target type.</param>
    /// <param name="parameter">Optional converter parameter (unused).</param>
    /// <param name="culture">Culture info for conversion.</param>
    /// <returns>Formatted timer text, or <c>00:00</c> when input is invalid.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return time.ToString(@"mm\:ss");
        }
        return "00:00";
    }

    /// <summary>
    /// Reverse conversion is not supported.
    /// </summary>
    /// <param name="value">Source value.</param>
    /// <param name="targetType">Target type.</param>
    /// <param name="parameter">Optional parameter.</param>
    /// <param name="culture">Culture info.</param>
    /// <returns>Never returns; always throws.</returns>
    /// <exception cref="NotImplementedException">Always thrown for one-way conversion.</exception>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
