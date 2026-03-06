using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Views;

public partial class AnalyticsDashboardPage : ContentPage
{
    public AnalyticsDashboardPage(AnalyticsViewModel vm) { InitializeComponent(); BindingContext = vm; }
}
