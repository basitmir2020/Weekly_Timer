namespace WeeklyTimetable;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("CheckInPage",      typeof(WeeklyTimetable.Views.CheckInPage));
        Routing.RegisterRoute("BlockDetailSheet", typeof(WeeklyTimetable.Views.BlockDetailSheet));
        Routing.RegisterRoute("ProfilesPage",     typeof(WeeklyTimetable.Views.ProfilesPage));
        Routing.RegisterRoute("EditBlockPage",    typeof(WeeklyTimetable.Views.EditBlockPage));
        Routing.RegisterRoute("ProductivityPage", typeof(WeeklyTimetable.Views.ProductivityPage));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Defer navigation so the Shell is fully rendered before navigating (prevents crash)
        Dispatcher.DispatchAsync(async () =>
        {
            bool done = Preferences.Get("onboarding_done", false);
            if (!done)
            {
                await GoToAsync("//OnboardingPage");
            }
        });
    }
}
