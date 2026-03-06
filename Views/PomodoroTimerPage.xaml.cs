using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Views;

public partial class PomodoroTimerPage : ContentPage
{
    public PomodoroTimerPage(PomodoroViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
