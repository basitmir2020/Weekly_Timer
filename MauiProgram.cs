using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.Maui.Audio;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui.Controls.Hosting;
using WeeklyTimetable.Services;
using WeeklyTimetable.ViewModels;
using WeeklyTimetable.Views;

namespace WeeklyTimetable;

public static class MauiProgram
{
    /// <summary>
    /// Builds and configures the MAUI application host, including DI registrations and UI toolkit setup.
    /// </summary>
    /// <returns>Configured <see cref="MauiApp"/> instance.</returns>
    /// <remarks>
    /// Side effects: registers fonts, services, view models, pages, and platform integrations in the host container.
    /// </remarks>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseLocalNotification()
            .UseSkiaSharp()
            .AddAudio()
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
        builder.Services.AddSingleton<IAlarmService,         AlarmService>();
        builder.Services.AddSingleton<IStreakService,        StreakService>();
        builder.Services.AddSingleton<IProfileService,       ProfileService>();
        builder.Services.AddSingleton<IExportService,        ExportService>();
        builder.Services.AddSingleton<ISupabaseSyncService,  SupabaseSyncService>();

#if ANDROID
        builder.Services.AddSingleton<IAlarmSoundPickerService, WeeklyTimetable.Platforms.Android.AlarmSoundPickerService>();
        builder.Services.AddSingleton<IAlarmSchedulerService,   WeeklyTimetable.Platforms.Android.AlarmSchedulerService>();
#else
        builder.Services.AddSingleton<IAlarmSoundPickerService, StubAlarmSoundPickerService>();
        builder.Services.AddSingleton<IAlarmSchedulerService,   StubAlarmSchedulerService>();
#endif

        // ── ViewModels ────────────────────────────────────────────
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddTransient<DayOverviewViewModel>();
        builder.Services.AddTransient<PomodoroViewModel>();
        builder.Services.AddTransient<StreakViewModel>();
        builder.Services.AddTransient<AnalyticsViewModel>();
        builder.Services.AddTransient<CheckInViewModel>();
        builder.Services.AddTransient<GoalsViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();

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
        builder.Services.AddTransient<BlockDetailSheet>();

        // AppShell registered last so its ctor resolves pages from DI
        builder.Services.AddSingleton<AppShell>();

        return builder.Build();
    }
}
