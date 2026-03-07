using Microsoft.Extensions.DependencyInjection;

namespace WeeklyTimetable;

public partial class App : Application
{
    /// <summary>
    /// Creates the application root and resolves the shell from dependency injection.
    /// </summary>
    /// <param name="services">Service provider used to resolve root UI dependencies.</param>
    /// <remarks>
    /// Side effects: initializes XAML resources and sets <see cref="Application.MainPage"/>.
    /// </remarks>
    public App(IServiceProvider services)
    {
        InitializeComponent();

        ApplyTheme();
        ApplyFontSize();

        MainPage = services.GetRequiredService<AppShell>();
    }

    private void ApplyTheme()
    {
        var theme = Preferences.Get("theme", "Dark");
        UserAppTheme = theme switch
        {
            "Light" => AppTheme.Light,
            "System" => AppTheme.Unspecified,
            _ => AppTheme.Dark
        };
    }

    private void ApplyFontSize()
    {
        var fontSize = Preferences.Get("font_size", "Medium");
        ApplyFontSize(fontSize);
    }

    public static void ApplyFontSize(string size)
    {
        if (Application.Current == null) return;

        double baseSize = size switch
        {
            "Small" => 12,
            "Large" => 17,
            _ => 14 // Medium
        };

        Application.Current.Resources["BaseFontSize"]   = baseSize;
        Application.Current.Resources["SmallFontSize"]  = baseSize * 0.8;
        Application.Current.Resources["HeaderFontSize"] = baseSize * 1.45;
        Application.Current.Resources["HeroFontSize"]   = baseSize * 2.0;
    }
}
