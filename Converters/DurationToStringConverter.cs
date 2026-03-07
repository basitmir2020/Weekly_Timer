using System.Globalization;

namespace WeeklyTimetable.Converters;

public class DurationToStringConverter : IValueConverter
{
    /// <summary>
    /// Converts a duration in minutes into compact human-readable text.
    /// </summary>
    /// <param name="value">Duration value in minutes.</param>
    /// <param name="targetType">Requested target type.</param>
    /// <param name="parameter">Optional converter parameter (unused).</param>
    /// <param name="culture">Culture info for conversion.</param>
    /// <returns>Formatted duration text (for example, <c>1h 30m</c>) or empty string.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int minutes && minutes > 0)
        {
            int h = minutes / 60;
            int m = minutes % 60;
            if (h > 0 && m > 0) return $"{h}h {m}m";
            if (h > 0) return $"{h}h";
            return $"{m}m";
        }
        return string.Empty;
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
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
