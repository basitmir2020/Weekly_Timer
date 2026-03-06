using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Views;

public partial class GoalsPage : ContentPage
{
    public GoalsPage(GoalsViewModel vm) { InitializeComponent(); BindingContext = vm; }
}
