using System.Globalization;

namespace WeeklyTimetable.Converters;

public class PercentToArcConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double pct)
        {
            return (pct / 100) * 360;
        }
        else if (value is int pctInt)
        {
             return (pctInt / 100.0) * 360;
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
