using System.Globalization;

namespace WeeklyTimetable.Converters;

public class Time24To12HrConverter : IValueConverter
{
    /// <summary>
    /// Converts 24-hour time text into 12-hour display format.
    /// </summary>
    /// <param name="value">Input time string (supports <c>HH:mm</c> and <c>H:mm</c>).</param>
    /// <param name="targetType">Requested target type.</param>
    /// <param name="parameter">Optional converter parameter (unused).</param>
    /// <param name="culture">Culture info for conversion.</param>
    /// <returns>Formatted 12-hour time string, or original fallback text when parsing fails.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string time24)
        {
            if (DateTime.TryParseExact(time24, "HH:mm", null, DateTimeStyles.None, out DateTime dt))
                return dt.ToString("h:mm tt");
            if (DateTime.TryParseExact(time24, "H:mm", null, DateTimeStyles.None, out DateTime dt2))
                return dt2.ToString("h:mm tt");
        }
        return value?.ToString() ?? string.Empty;
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
