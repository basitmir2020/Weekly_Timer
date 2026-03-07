using Android.App;
using Android.Runtime;

namespace WeeklyTimetable;

[Application]
public class MainApplication : MauiApplication
{
	/// <summary>
	/// Creates the Android application host wrapper.
	/// </summary>
	/// <param name="handle">Native Android handle.</param>
	/// <param name="ownership">Ownership semantics for the native handle.</param>
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	/// <summary>
	/// Creates the shared MAUI app instance for Android startup.
	/// </summary>
	/// <returns>Configured <see cref="MauiApp"/>.</returns>
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
