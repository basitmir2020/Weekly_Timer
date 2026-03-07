using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;

namespace WeeklyTimetable;

/// <summary>
/// A native Android Foreground Service that plays looping audio and vibrates
/// until the user explicitly stops it via a notification action button.
/// </summary>
[Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeMediaPlayback)]
public sealed class AlarmForegroundService : Service
{
    private MediaPlayer? _mediaPlayer;
    private Vibrator? _vibrator;
    private const string ChannelId = "alarm_foreground_channel";
    private const int NotificationId = 1001;

    public const string ActionStop = "STOP_ALARM_ACTION";

    public override IBinder? OnBind(Intent? intent) => null;

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        if (intent?.Action == ActionStop)
        {
            StopAlarm();
            return StartCommandResult.NotSticky;
        }

        string title = intent?.GetStringExtra("title") ?? "Scheduled Block Starting";
        string message = intent?.GetStringExtra("message") ?? "Time to start your next activity!";

        StartForeground(NotificationId, CreateNotification(title, message));
        PlayAlarm();
        StartVibration();

        return StartCommandResult.Sticky;
    }

    private void PlayAlarm()
    {
        try
        {
            var uriString = Preferences.Get("alarm_sound_uri", null);
            global::Android.Net.Uri? alarmUri;

            if (!string.IsNullOrWhiteSpace(uriString))
                alarmUri = global::Android.Net.Uri.Parse(uriString);
            else
                alarmUri = RingtoneManager.GetDefaultUri(RingtoneType.Alarm);

            if (alarmUri == null) return;

            _mediaPlayer = MediaPlayer.Create(this, alarmUri);
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Looping = true;
                _mediaPlayer.Start();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AlarmForegroundService] Play error: {ex.Message}");
        }
    }

    private void StartVibration()
    {
        _vibrator = GetSystemService(VibratorService) as Vibrator;
        if (_vibrator == null || !_vibrator.HasVibrator) return;

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            long[] pattern = { 0, 800, 1500 };
            _vibrator.Vibrate(VibrationEffect.CreateWaveform(pattern, 0));
        }
        else
        {
            _vibrator.Vibrate(800);
        }
    }

    private Notification CreateNotification(string title, string message)
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(ChannelId, "Alarms", NotificationImportance.Max)
            {
                Description = "Persistent alarms for scheduled blocks",
                LockscreenVisibility = NotificationVisibility.Public
            };
            var manager = GetSystemService(NotificationService) as NotificationManager;
            manager?.CreateNotificationChannel(channel);
        }

        var stopIntent = new Intent(this, typeof(AlarmForegroundService));
        stopIntent.SetAction(ActionStop);
        var stopPendingIntent = PendingIntent.GetService(this, 0, stopIntent, 
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        return new NotificationCompat.Builder(this, ChannelId)
            .SetContentTitle(title)
            .SetContentText(message)
            .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)
            .SetPriority(NotificationCompat.PriorityMax)
            .SetCategory(NotificationCompat.CategoryAlarm)
            .SetOngoing(true)
            .SetAutoCancel(false)
            .SetVisibility(NotificationCompat.VisibilityPublic)
            .AddAction(global::Android.Resource.Drawable.IcMenuCloseClearCancel, "STOP", stopPendingIntent)
            .SetContentIntent(stopPendingIntent)
            .Build();
    }

    private void StopAlarm()
    {
        try { _mediaPlayer?.Stop(); _mediaPlayer?.Release(); } catch { }
        try { _vibrator?.Cancel(); } catch { }
        _mediaPlayer = null;
        StopForeground(true);
        StopSelf();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        StopAlarm();
    }
}
