using System.Globalization;
using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Converters;

public class ActiveDayOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string dayAbbreviation)
        {
            if (Application.Current?.MainPage is NavigationPage navPage && navPage.CurrentPage.BindingContext is MainViewModel vm)
            {
                if (vm.ActiveDay.StartsWith(dayAbbreviation, StringComparison.OrdinalIgnoreCase))
                    return 1.0;
            }
            else if (Application.Current?.MainPage is Shell shell && shell.CurrentPage?.BindingContext is MainViewModel vm2)
            {
                 if (vm2.ActiveDay.StartsWith(dayAbbreviation, StringComparison.OrdinalIgnoreCase))
                    return 1.0;
            }
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
