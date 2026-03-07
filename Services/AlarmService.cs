using Plugin.Maui.Audio;

namespace WeeklyTimetable.Services;

/// <summary>
/// Provides a looping alarm (audio + vibration) that plays until the user explicitly stops it.
/// On Android, plays the URI saved in the <c>alarm_sound_uri</c> Preference via Android MediaPlayer.
/// Falls back to a generated sine-wave WAV on non-Android or when no URI is saved.
/// </summary>
public sealed class AlarmService : IAlarmService
{
    private readonly IAudioManager _audioManager;

    private volatile bool _isRinging;
    private IAudioPlayer? _player;

#if ANDROID
    private Android.Media.MediaPlayer? _mediaPlayer;
#endif

    private CancellationTokenSource? _vibrationCts;

    public bool IsAlarmRinging => _isRinging;

    public AlarmService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    /// <summary>Starts the looping alarm. No-op when already ringing.</summary>
    /// <remarks>Side effects: plays audio in a loop, starts vibration background task.</remarks>
    public void StartAlarm()
    {
        if (_isRinging) return;
        _isRinging = true;
        _ = Task.Run(StartAlarmCoreAsync);
    }

    /// <summary>Stops the looping alarm immediately.</summary>
    /// <remarks>Side effects: stops audio playback and vibration loop.</remarks>
    public void StopAlarm()
    {
        _isRinging = false;

        // Stop Plugin.Maui.Audio player (fallback path)
        try { _player?.Stop(); }    catch { /* best effort */ }
        try { _player?.Dispose(); } catch { /* best effort */ }
        _player = null;

#if ANDROID
        // Stop Android MediaPlayer (primary path on Android)
        try
        {
            _mediaPlayer?.Stop();
            _mediaPlayer?.Release();
        }
        catch { /* best effort */ }
        _mediaPlayer = null;
#endif

        _vibrationCts?.Cancel();
        _vibrationCts?.Dispose();
        _vibrationCts = null;

        try { Vibration.Default.Cancel(); } catch { /* best effort */ }
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task StartAlarmCoreAsync()
    {
        bool playedViaAndroid = false;

#if ANDROID
        playedViaAndroid = TryPlayAndroidUri();
#endif

        // Fallback: generated sine-wave WAV via Plugin.Maui.Audio
        if (!playedViaAndroid)
        {
            try
            {
                string wavPath = await EnsureAlarmWavAsync();
                using var stream = File.OpenRead(wavPath);
                _player = _audioManager.CreatePlayer(stream);
                _player.Loop   = true;
                _player.Volume = 1.0;
                _player.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AlarmService] Audio fallback error: {ex.Message}");
            }
        }

        // Vibration loop — runs independently even if audio fails.
        _vibrationCts = new CancellationTokenSource();
        var token = _vibrationCts.Token;

        try
        {
            while (!token.IsCancellationRequested && _isRinging)
            {
                try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(800)); }
                catch { /* vibration not available */ }

                await Task.Delay(1500, token);
            }
        }
        catch (OperationCanceledException) { /* expected on stop */ }
    }

#if ANDROID
    /// <summary>
    /// Attempts to play the user-chosen alarm URI via Android MediaPlayer.
    /// </summary>
    /// <returns><c>true</c> when playback starts successfully.</returns>
    /// <remarks>
    /// Reads <c>alarm_sound_uri</c> from Preferences (set by Settings picker).
    /// Falls back to the system default alarm URI when no preference is saved.
    /// Side effects: allocates a MediaPlayer and begins audio playback.
    /// </remarks>
    private bool TryPlayAndroidUri()
    {
        try
        {
            var uriString = Preferences.Get("alarm_sound_uri", null);

            Android.Net.Uri? androidUri;
            if (!string.IsNullOrWhiteSpace(uriString))
            {
                androidUri = Android.Net.Uri.Parse(uriString);
            }
            else
            {
                // No user selection — use the system default alarm sound.
                androidUri = Android.Media.RingtoneManager.GetDefaultUri(
                    Android.Media.RingtoneType.Alarm);
            }

            if (androidUri == null) return false;

            var ctx = Android.App.Application.Context;
            _mediaPlayer = Android.Media.MediaPlayer.Create(ctx, androidUri);
            if (_mediaPlayer == null) return false;

            _mediaPlayer.Looping = true;
            _mediaPlayer.Start();
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AlarmService] Android URI play error: {ex.Message}");
            return false;
        }
    }
#endif

    /// <summary>
    /// Generates a sine-wave WAV alarm tone and caches it in the app data directory.
    /// </summary>
    private static async Task<string> EnsureAlarmWavAsync()
    {
        string path = Path.Combine(FileSystem.AppDataDirectory, "alarm_tone.wav");
        if (File.Exists(path)) return path;

        await Task.Run(() =>
        {
            const int sampleRate   = 44100;
            const double frequency = 880.0;   // A5
            const int durationSec  = 3;
            int numSamples = sampleRate * durationSec;
            short[] samples = new short[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                double envelope = 1.0;
                int fadeLen = sampleRate / 20; // 50 ms fade
                if (i < fadeLen)                 envelope = (double)i / fadeLen;
                else if (i > numSamples - fadeLen) envelope = (double)(numSamples - i) / fadeLen;

                double t = (double)i / sampleRate;
                samples[i] = (short)(Math.Sin(2 * Math.PI * frequency * t) * 32000 * envelope);
            }

            WriteWav(path, samples, sampleRate);
        });

        return path;
    }

    private static void WriteWav(string path, short[] samples, int sampleRate)
    {
        const int bitsPerSample = 16, channels = 1;
        int dataSize   = samples.Length * sizeof(short);
        int byteRate   = sampleRate * channels * bitsPerSample / 8;
        int blockAlign = channels * bitsPerSample / 8;

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(fs);

        bw.Write(new[] { 'R','I','F','F' }); bw.Write(36 + dataSize);
        bw.Write(new[] { 'W','A','V','E' });
        bw.Write(new[] { 'f','m','t',' ' }); bw.Write(16);
        bw.Write((short)1); bw.Write((short)channels);
        bw.Write(sampleRate); bw.Write(byteRate);
        bw.Write((short)blockAlign); bw.Write((short)bitsPerSample);
        bw.Write(new[] { 'd','a','t','a' }); bw.Write(dataSize);
        foreach (var s in samples) bw.Write(s);
    }
}
