using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Views;

public partial class PomodoroTimerPage : ContentPage
{
    /// <summary>
    /// Initializes pomodoro timer page and assigns injected view model.
    /// </summary>
    /// <param name="viewModel">Pomodoro view model for timer state and commands.</param>
    public PomodoroTimerPage(PomodoroViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
