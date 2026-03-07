using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Views;

public partial class CheckInPage : ContentPage
{
    /// <summary>
    /// Initializes check-in page and assigns injected check-in view model.
    /// </summary>
    /// <param name="vm">Check-in view model for mood/energy journaling.</param>
    public CheckInPage(CheckInViewModel vm) { InitializeComponent(); BindingContext = vm; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CheckInViewModel vm)
        {
            await vm.EnsureLoadedAsync();
        }
    }
}
