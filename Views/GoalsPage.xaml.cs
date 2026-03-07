using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Views;

public partial class GoalsPage : ContentPage
{
    /// <summary>
    /// Initializes weekly goals page and assigns injected goals view model.
    /// </summary>
    /// <param name="vm">Goals view model for weekly target management.</param>
    public GoalsPage(GoalsViewModel vm) { InitializeComponent(); BindingContext = vm; }
}
