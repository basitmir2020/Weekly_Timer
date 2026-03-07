namespace WeeklyTimetable;

public partial class AppShell : Shell
{
    /// <summary>
    /// Initializes app shell and registers named navigation routes.
    /// </summary>
    /// <remarks>
    /// Side effects: registers routes globally in MAUI routing table.
    /// </remarks>
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("CheckInPage",      typeof(WeeklyTimetable.Views.CheckInPage));
        Routing.RegisterRoute("BlockDetailSheet", typeof(WeeklyTimetable.Views.BlockDetailSheet));
        Routing.RegisterRoute("ProfilesPage",     typeof(WeeklyTimetable.Views.ProfilesPage));
        Routing.RegisterRoute("EditBlockPage",    typeof(WeeklyTimetable.Views.EditBlockPage));
        Routing.RegisterRoute("ProductivityPage", typeof(WeeklyTimetable.Views.ProductivityPage));
    }

    /// <summary>
    /// Handles shell appearance and redirects first-time users to onboarding.
    /// </summary>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: may perform shell navigation to onboarding route.
    /// </remarks>
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Defer navigation so the Shell is fully rendered before navigating (prevents crash)
        Dispatcher.DispatchAsync(async () =>
        {
            bool done = Preferences.Get("onboarding_done", false);
            if (!done)
            {
                // Absolute route ensures onboarding becomes the active root flow.
                await GoToAsync("//OnboardingPage");
            }
        });
    }
}
