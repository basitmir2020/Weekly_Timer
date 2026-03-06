using System.Globalization;
using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Converters;

public class ActiveDayTextColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string dayAbbreviation)
        {
            // We need to resolve App.Current.MainPage's BindingContext to compare against ActiveDay
            if (Application.Current?.MainPage is NavigationPage navPage && navPage.CurrentPage.BindingContext is MainViewModel vm)
            {
                if (vm.ActiveDay.StartsWith(dayAbbreviation, StringComparison.OrdinalIgnoreCase))
                {
                    return Color.FromArgb("#f1f5f9"); // TextHeading
                }
            }
            else if (Application.Current?.MainPage is Shell shell && shell.CurrentPage?.BindingContext is MainViewModel vm2)
            {
                 if (vm2.ActiveDay.StartsWith(dayAbbreviation, StringComparison.OrdinalIgnoreCase))
                {
                    return Color.FromArgb("#f1f5f9"); 
                }
            }
        }
        return Color.FromArgb("#334155"); // TextMuted
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
