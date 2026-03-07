using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace WeeklyTimetable;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    /// <summary>
    /// Registered by <see cref="WeeklyTimetable.Platforms.Android.AlarmSoundPickerService"/>
    /// to receive the ringtone picker result.
    /// </summary>
    internal static Action<int, Result, Intent?>? ActivityResultCallback;

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        ActivityResultCallback?.Invoke(requestCode, resultCode, data);
    }
}
