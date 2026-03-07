using Android.App;
using Android.Content;
using Android.Media;
using WeeklyTimetable.Services;

namespace WeeklyTimetable.Platforms.Android;

/// <summary>
/// Android implementation of <see cref="IAlarmSoundPickerService"/>.
/// Launches the system RingtoneManager alarm picker and awaits the result
/// via <see cref="MainActivity.OnActivityResult"/>.
/// </summary>
public sealed class AlarmSoundPickerService : IAlarmSoundPickerService
{
    private const int RequestCode = 0xA1A2;

    /// <summary>
    /// Opens Android's built-in alarm ringtone picker and returns the chosen URI string.
    /// Returns <c>null</c> when the user cancels.
    /// </summary>
    /// <remarks>
    /// Side effects: registers a one-shot callback on <see cref="MainActivity.ActivityResultCallback"/>,
    /// starts the system ringtone picker Activity, and resolves the TCS when the result arrives.
    /// </remarks>
    public Task<string?> PickAlarmSoundAsync()
    {
        var tcs = new TaskCompletionSource<string?>();

        var activity = Platform.CurrentActivity as global::Android.App.Activity
            ?? throw new InvalidOperationException("No current Activity.");

        // Register our one-shot callback before the picker is started.
        MainActivity.ActivityResultCallback = (reqCode, resultCode, data) =>
        {
            if (reqCode != RequestCode) return;

            // Unregister immediately — we only care about the first matching result.
            MainActivity.ActivityResultCallback = null;

            if (resultCode == Result.Ok && data != null)
            {
                var uri = data.GetParcelableExtra(RingtoneManager.ExtraRingtonePickedUri)
                    as global::Android.Net.Uri;
                tcs.TrySetResult(uri?.ToString());
            }
            else
            {
                tcs.TrySetResult(null); // cancelled or no data
            }
        };

        // Build and launch the picker intent.
        var currentUriStr = Preferences.Get("alarm_sound_uri", null);

        var intent = new Intent(RingtoneManager.ActionRingtonePicker);
        intent.PutExtra(RingtoneManager.ExtraRingtoneType, (int)RingtoneType.Alarm);
        intent.PutExtra(RingtoneManager.ExtraRingtoneShowSilent, false);
        intent.PutExtra(RingtoneManager.ExtraRingtoneShowDefault, true);
        intent.PutExtra(RingtoneManager.ExtraRingtoneTitle, "Select Alarm Sound");

        if (!string.IsNullOrWhiteSpace(currentUriStr))
        {
            var existingUri = global::Android.Net.Uri.Parse(currentUriStr);
            intent.PutExtra(RingtoneManager.ExtraRingtoneExistingUri, existingUri);
        }

        activity.StartActivityForResult(intent, RequestCode);

        return tcs.Task;
    }

    /// <summary>
    /// Returns a human-readable name for the given ringtone URI, or "Default" when unavailable.
    /// </summary>
    public string GetSoundName(string? uri)
    {
        if (string.IsNullOrWhiteSpace(uri)) return "Default";
        try
        {
            var ctx = global::Android.App.Application.Context;
            var androidUri = global::Android.Net.Uri.Parse(uri);
            var ringtone = RingtoneManager.GetRingtone(ctx, androidUri);
            return ringtone?.GetTitle(ctx) ?? "Custom";
        }
        catch
        {
            return "Custom";
        }
    }
}
