using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Views;

public partial class ProfilesPage : ContentPage
{
    /// <summary>
    /// Initializes profiles page and assigns injected profile view model.
    /// </summary>
    /// <param name="vm">Profile view model for profile activation/deletion.</param>
    public ProfilesPage(ProfileViewModel vm) { InitializeComponent(); BindingContext = vm; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ProfileViewModel vm)
        {
            await vm.EnsureLoadedAsync();
        }
    }
}
