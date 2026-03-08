using System.Globalization;

namespace WeeklyTimetable.Converters;

public class BoolToStrikethroughConverter : IValueConverter
{
    /// <summary>
    /// Converts completion state into text decoration for strike-through presentation.
    /// </summary>
    /// <param name="value">Completion flag value or Enum value.</param>
    /// <param name="targetType">Requested target type.</param>
    /// <param name="parameter">Optional converter parameter (expected enum string if value is enum).</param>
    /// <param name="culture">Culture info for conversion.</param>
    /// <returns>Strike-through decoration when completed; otherwise none.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter != null && value != null)
        {
            if (value.ToString() == parameter.ToString())
            {
                return TextDecorations.Strikethrough;
            }
            return TextDecorations.None;
        }

        if (value is bool isCompleted && isCompleted)
        {
            return TextDecorations.Strikethrough;
        }
        return TextDecorations.None;
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
