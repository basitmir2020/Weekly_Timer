using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui.Controls.Hosting;
using WeeklyTimetable.Services;
using WeeklyTimetable.ViewModels;
using WeeklyTimetable.Views;

namespace WeeklyTimetable;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseLocalNotification()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf",  "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // ── Services ──────────────────────────────────────────────
        builder.Services.AddSingleton<IPersistenceService,  PersistenceService>();
        builder.Services.AddSingleton<IDatabaseService,     DatabaseService>();
        builder.Services.AddSingleton<WeeklyTimetable.Services.INotificationService, WeeklyTimetable.Services.NotificationService>();
        builder.Services.AddSingleton<IStreakService,        StreakService>();
        builder.Services.AddSingleton<IProfileService,       ProfileService>();
        builder.Services.AddSingleton<IExportService,        ExportService>();
        builder.Services.AddSingleton<ISupabaseSyncService,  SupabaseSyncService>();

        // ── ViewModels ────────────────────────────────────────────
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<DayOverviewViewModel>();
        builder.Services.AddTransient<PomodoroViewModel>();
        builder.Services.AddTransient<StreakViewModel>();
        builder.Services.AddTransient<AnalyticsViewModel>();
        builder.Services.AddTransient<CheckInViewModel>();
        builder.Services.AddTransient<GoalsViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<EditBlockViewModel>();

        // ── Views / Pages ─────────────────────────────────────────
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<OnboardingPage>();
        builder.Services.AddTransient<PomodoroTimerPage>();
        builder.Services.AddTransient<AnalyticsDashboardPage>();
        builder.Services.AddTransient<CheckInPage>();
        builder.Services.AddTransient<GoalsPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<ProfilesPage>();
        builder.Services.AddTransient<EditBlockPage>();
        builder.Services.AddTransient<ProductivityPage>();

        // AppShell registered last so its ctor resolves pages from DI
        builder.Services.AddSingleton<AppShell>();

        return builder.Build();
    }
}
