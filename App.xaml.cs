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
        MainPage = services.GetRequiredService<AppShell>();
    }
}
