using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Views;

public partial class SettingsPage : ContentPage
{
    /// <summary>
    /// Initializes settings page and assigns injected settings view model.
    /// </summary>
    /// <param name="vm">Settings view model handling preferences and backup/restore actions.</param>
    public SettingsPage(SettingsViewModel vm) { InitializeComponent(); BindingContext = vm; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SettingsViewModel vm)
        {
            await vm.EnsureMasterScheduleLoadedAsync();
        }
    }
}
