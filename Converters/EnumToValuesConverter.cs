using System.Globalization;

namespace WeeklyTimetable.Converters;

public class EnumToValuesConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is Type enumType && enumType.IsEnum)
        {
            return Enum.GetValues(enumType);
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
