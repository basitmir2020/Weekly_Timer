using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Views;

public partial class ProfilesPage : ContentPage
{
    public ProfilesPage(ProfileViewModel vm) { InitializeComponent(); BindingContext = vm; }
}
