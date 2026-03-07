using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WeeklyTimetable.Views;

public partial class OnboardingPage : ContentPage
{
    /// <summary>
    /// Initializes onboarding page and assigns local onboarding view model as binding context.
    /// </summary>
    public OnboardingPage()
    {
        InitializeComponent();
        BindingContext = new OnboardingViewModel();
    }
}

public class OnboardingSlide
{
    public string Emoji { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body  { get; set; } = string.Empty;
}

public partial class OnboardingViewModel : ObservableObject
{
    public List<OnboardingSlide> Slides { get; } = new()
    {
        new() { Emoji = "📅", Title = "Weekly Blueprint",
                Body  = "Consistency beats intensity.\nTrack every block of your day and build momentum over time." },
        new() { Emoji = "✅", Title = "How It Works",
                Body  = "Tap to check off blocks as you complete them.\nSwipe for notes. Use the timer on study blocks." },
        new() { Emoji = "🗓️", Title = "Your Schedule",
                Body  = "7 days. DSA in the morning. Web Dev in the evening.\nWeekends for projects and deep revision." },
    };

    /// <summary>
    /// Marks onboarding as complete and navigates to the main application page.
    /// </summary>
    /// <returns>A task that completes after preference update and navigation.</returns>
    /// <remarks>
    /// Side effects: writes onboarding completion preference and performs shell navigation.
    /// </remarks>
    [RelayCommand]
    private async Task FinishAsync()
    {
        Preferences.Set("onboarding_done", true);
        await Shell.Current.GoToAsync("//MainPage");
    }
}
