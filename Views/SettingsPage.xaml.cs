using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel vm) { InitializeComponent(); BindingContext = vm; }
}
