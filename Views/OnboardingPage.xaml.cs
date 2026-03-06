using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WeeklyTimetable.Views;

public partial class OnboardingPage : ContentPage
{
    public OnboardingPage()
    {
        InitializeComponent();
        BindingContext = new OnboardingViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Connect IndicatorView to CarouselView programmatically
        OnboardingCarousel.IndicatorView = pageIndicator;
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

    [RelayCommand]
    private async Task FinishAsync()
    {
        Preferences.Set("onboarding_done", true);
        await Shell.Current.GoToAsync("//MainPage");
    }
}
