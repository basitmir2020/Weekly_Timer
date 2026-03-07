using Microsoft.Extensions.DependencyInjection;

namespace WeeklyTimetable;

public partial class App : Application
{
    public App(IServiceProvider services)
    {
        InitializeComponent();
        MainPage = services.GetRequiredService<AppShell>();
    }

    protected override void OnStart()
    {
        base.OnStart();
        ApplyStartupTheme();
        ApplyInitialFontSize();
    }

    private void ApplyStartupTheme()
    {
        var theme = Preferences.Get("theme", "Dark");
        UserAppTheme = theme switch
        {
            "Light" => AppTheme.Light,
            "System" => AppTheme.Unspecified,
            _ => AppTheme.Dark
        };
    }

    private void ApplyInitialFontSize()
    {
        var fontSize = Preferences.Get("font_size", "Medium");
        ApplyGlobalFontSize(fontSize);
    }

    public static void ApplyGlobalFontSize(string size)
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
