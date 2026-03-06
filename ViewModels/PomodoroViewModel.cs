using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WeeklyTimetable.ViewModels;

public enum TimerMode
{
    Focus,
    ShortBreak,
    LongBreak,
    Custom
}

public partial class PomodoroViewModel : ObservableObject
{
    private IDispatcherTimer _timer;
    
    [ObservableProperty]
    private int _secondsRemaining;

    [ObservableProperty]
    private int _cycleCount;

    [ObservableProperty]
    private TimerMode _currentMode;

    [ObservableProperty]
    private string _timerStateLabel = "Ready to Focus";

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private double _progressPct;

    [ObservableProperty]
    private double _focusMinutes = 25;

    [ObservableProperty]
    private double _shortBreakMinutes = 5;

    [ObservableProperty]
    private double _longBreakMinutes = 20;

    [ObservableProperty]
    private double _customMinutes = 25;

    [ObservableProperty]
    private double _customSeconds = 0;

    [ObservableProperty]
    private bool _isCustomTimerVisible;

    private int _totalSecondsForCurrentMode;

    public PomodoroViewModel()
    {
        _timer = Application.Current.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;
        
        SetMode(TimerMode.Focus);
    }

    private void SetMode(TimerMode mode)
    {
        CurrentMode = mode;
        _totalSecondsForCurrentMode = GetSecondsForMode(mode);
        
        SecondsRemaining = _totalSecondsForCurrentMode;
        ProgressPct = 100.0;
        
        TimerStateLabel = mode switch
        {
            TimerMode.Focus => "Deep Focus",
            TimerMode.ShortBreak => "Short Break",
            TimerMode.LongBreak => "Long Break",
            TimerMode.Custom => "Custom Timer",
            _ => "Deep Focus"
        };
    }

    private int GetSecondsForMode(TimerMode mode)
    {
        if (mode == TimerMode.Custom)
        {
            return (int)Math.Round((CustomMinutes * 60) + CustomSeconds);
        }

        var minutes = mode switch
        {
            TimerMode.Focus => FocusMinutes,
            TimerMode.ShortBreak => ShortBreakMinutes,
            TimerMode.LongBreak => LongBreakMinutes,
            _ => 25
        };

        return (int)Math.Round(Math.Max(1, minutes) * 60);
    }

    [RelayCommand]
    private void StartPauseTimer()
    {
        if (IsRunning)
        {
            _timer.Stop();
            IsRunning = false;
        }
        else
        {
            _timer.Start();
            IsRunning = true;
        }
    }

    [RelayCommand]
    private void SkipTimer()
    {
        _timer.Stop();
        IsRunning = false;
        TransitionToNextPhase();
    }

    [RelayCommand]
    private void ResetTimer()
    {
        _timer.Stop();
        IsRunning = false;
        CycleCount = 0;
        SetMode(TimerMode.Focus);
    }

    [RelayCommand]
    private void ApplyCustomTimer()
    {
        _timer.Stop();
        IsRunning = false;
        SetMode(TimerMode.Custom);
        IsCustomTimerVisible = false;
    }

    [RelayCommand]
    private void ShowCustomTimer()
    {
        IsCustomTimerVisible = true;
    }

    [RelayCommand]
    private void CloseCustomTimer()
    {
        IsCustomTimerVisible = false;
    }

    [RelayCommand]
    private void SetFocusMode()
    {
        _timer.Stop();
        IsRunning = false;
        SetMode(TimerMode.Focus);
    }

    [RelayCommand]
    private void SetBreakMode()
    {
        _timer.Stop();
        IsRunning = false;
        SetMode(TimerMode.ShortBreak);
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        if (SecondsRemaining > 0)
        {
            SecondsRemaining--;
            ProgressPct = ((double)SecondsRemaining / _totalSecondsForCurrentMode) * 100;
        }
        else
        {
            _timer.Stop();
            IsRunning = false;
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            TransitionToNextPhase();
        }
    }

    private void TransitionToNextPhase()
    {
        if (CurrentMode == TimerMode.Focus)
        {
            CycleCount++;
            if (CycleCount % 4 == 0)
            {
                SetMode(TimerMode.LongBreak);
            }
            else
            {
                SetMode(TimerMode.ShortBreak);
            }
        }
        else
        {
            SetMode(TimerMode.Focus);
        }
    }
}
