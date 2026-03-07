using System.Globalization;

namespace WeeklyTimetable.Converters;

public class BoolToStringConverter : IValueConverter
{
    public string TrueString { get; set; } = "True";
    public string FalseString { get; set; } = "False";

    /// <summary>
    /// Converts a boolean value into display text, with optional <c>true|false</c> override parameter.
    /// </summary>
    /// <param name="value">Boolean source value.</param>
    /// <param name="targetType">Requested target type.</param>
    /// <param name="parameter">Optional text override in format <c>TrueText|FalseText</c>.</param>
    /// <param name="culture">Culture info for conversion.</param>
    /// <returns>Configured true/false string result.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string trueStr = TrueString;
        string falseStr = FalseString;

        if (parameter is string paramStr && paramStr.Contains('|'))
        {
            var parts = paramStr.Split('|');
            if (parts.Length >= 2)
            {
                // Parameterized labels allow reuse across many UI contexts without extra converters.
                trueStr = parts[0];
                falseStr = parts[1];
            }
        }

        if (value is bool isTrue && isTrue)
        {
            return trueStr;
        }
        return falseStr;
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
