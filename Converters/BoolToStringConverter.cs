using System.Globalization;

namespace WeeklyTimetable.Converters;

public class BoolToStringConverter : IValueConverter
{
    public string TrueString { get; set; } = "True";
    public string FalseString { get; set; } = "False";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string trueStr = TrueString;
        string falseStr = FalseString;

        if (parameter is string paramStr && paramStr.Contains('|'))
        {
            var parts = paramStr.Split('|');
            if (parts.Length >= 2)
            {
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
