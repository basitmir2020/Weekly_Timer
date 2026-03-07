using Android.Content;
using Android.OS;

namespace WeeklyTimetable;

/// <summary>
/// Receives the alarm broadcast from AlarmManager and starts the Foreground Service.
/// </summary>
[BroadcastReceiver(Enabled = true, Exported = false)]
public sealed class AlarmReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null || intent == null) return;

        var serviceIntent = new Intent(context, typeof(AlarmForegroundService));
        serviceIntent.PutExtra("title", intent.GetStringExtra("title") ?? "Activity Starting");
        serviceIntent.PutExtra("message", intent.GetStringExtra("message") ?? "Go to schedule tab.");

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            context.StartForegroundService(serviceIntent);
        }
        else
        {
            context.StartService(serviceIntent);
        }
    }
}
