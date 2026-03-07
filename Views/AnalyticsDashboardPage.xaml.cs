using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Views;

public partial class AnalyticsDashboardPage : ContentPage
{
    /// <summary>
    /// Initializes analytics dashboard page and assigns injected analytics view model.
    /// </summary>
    /// <param name="vm">Analytics view model providing dashboard data.</param>
    public AnalyticsDashboardPage(AnalyticsViewModel vm) { InitializeComponent(); BindingContext = vm; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AnalyticsViewModel vm)
        {
            await vm.EnsureLoadedAsync();
        }
    }
}
