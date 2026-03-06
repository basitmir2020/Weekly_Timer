using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Views;

public partial class CheckInPage : ContentPage
{
    public CheckInPage(CheckInViewModel vm) { InitializeComponent(); BindingContext = vm; }
}
