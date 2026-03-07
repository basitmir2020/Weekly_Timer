using Foundation;

namespace WeeklyTimetable;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	/// <summary>
	/// Creates the shared MAUI app instance for iOS startup.
	/// </summary>
	/// <returns>Configured <see cref="MauiApp"/>.</returns>
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
