using System.Globalization;

namespace WeeklyTimetable.Converters;

public class PercentToWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double maxW = 35.0; // Based on the UI WidthRequest=35
        if (value is int pct)
        {
            return (pct / 100.0) * maxW;
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
