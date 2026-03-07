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
    private readonly IDispatcherTimer _timer;
    
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

    [ObservableProperty]
    private bool _isWarningActive;

    private int _totalSecondsForCurrentMode;

    private readonly Services.IAlarmService _alarmService;

    /// <summary>
    /// Initializes timer infrastructure and defaults the view model to focus mode.
    /// </summary>
    /// <param name="alarmService">Service used to play completion sounds.</param>
    /// <remarks>
    /// Side effects: creates a dispatcher timer and subscribes to its tick event.
    /// </remarks>
    public PomodoroViewModel(Services.IAlarmService alarmService)
    {
        _alarmService = alarmService;
        var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.GetForCurrentThread();
        _timer = dispatcher?.CreateTimer()
                 ?? throw new InvalidOperationException("No dispatcher is available for Pomodoro timer.");
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;
        
        SetMode(TimerMode.Focus);
    }

    /// <summary>
    /// Switches timer mode and resets countdown/progress values for that mode.
    /// </summary>
    /// <param name="mode">Target timer phase to activate.</param>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: updates mode, countdown seconds, progress percentage, and state label.
    /// </remarks>
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

    /// <summary>
    /// Resolves duration in seconds for a given mode, including custom minute/second input.
    /// </summary>
    /// <param name="mode">Timer mode to evaluate.</param>
    /// <returns>Total duration in seconds for the selected mode.</returns>
    private int GetSecondsForMode(TimerMode mode)
    {
        if (mode == TimerMode.Custom)
        {
            // Custom mode uses separate minute/second fields from the custom timer sheet.
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

    /// <summary>
    /// Toggles the timer between running and paused states.
    /// </summary>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: starts or stops the dispatcher timer and updates <see cref="IsRunning"/>.
    /// </remarks>
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

    /// <summary>
    /// Stops the warning sound and hides the stop button.
    /// </summary>
    [RelayCommand]
    private void StopWarning()
    {
        _alarmService.StopFocusWarningSound();
        IsWarningActive = false;
    }

    /// <summary>
    /// Stops the current phase and immediately transitions to the next timer phase.
    /// </summary>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: stops timer execution and mutates mode/cycle state through phase transition.
    /// </remarks>
    [RelayCommand]
    private void SkipTimer()
    {
        _timer.Stop();
        IsRunning = false;
        StopWarning();
        TransitionToNextPhase();
    }

    /// <summary>
    /// Resets the timer workflow back to initial focus mode and clears completed cycle count.
    /// </summary>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: stops timer and resets mode/cycle state.
    /// </remarks>
    [RelayCommand]
    private void ResetTimer()
    {
        _timer.Stop();
        IsRunning = false;
        StopWarning();
        CycleCount = 0;
        SetMode(TimerMode.Focus);
    }

    /// <summary>
    /// Applies the currently entered custom duration and returns to the timer view.
    /// </summary>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: stops timer, sets custom mode values, and hides the custom input UI.
    /// </remarks>
    [RelayCommand]
    private void ApplyCustomTimer()
    {
        _timer.Stop();
        IsRunning = false;
        SetMode(TimerMode.Custom);
        IsCustomTimerVisible = false;
    }

    /// <summary>
    /// Opens the custom timer configuration panel.
    /// </summary>
    /// <returns>None.</returns>
    [RelayCommand]
    private void ShowCustomTimer()
    {
        IsCustomTimerVisible = true;
    }

    /// <summary>
    /// Closes the custom timer configuration panel.
    /// </summary>
    /// <returns>None.</returns>
    [RelayCommand]
    private void CloseCustomTimer()
    {
        IsCustomTimerVisible = false;
    }

    /// <summary>
    /// Switches the timer to focus mode and ensures it is not running.
    /// </summary>
    /// <returns>None.</returns>
    [RelayCommand]
    private void SetFocusMode()
    {
        _timer.Stop();
        IsRunning = false;
        SetMode(TimerMode.Focus);
    }

    /// <summary>
    /// Switches the timer to short-break mode and ensures it is not running.
    /// </summary>
    /// <returns>None.</returns>
    [RelayCommand]
    private void SetBreakMode()
    {
        _timer.Stop();
        IsRunning = false;
        SetMode(TimerMode.ShortBreak);
    }

    /// <summary>
    /// Handles per-second timer ticks, updating countdown/progress and transitioning when time reaches zero.
    /// </summary>
    /// <param name="sender">Timer source.</param>
    /// <param name="e">Event arguments for the tick event.</param>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: mutates countdown state, triggers haptic feedback, and may change timer phase.
    /// </remarks>
    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (SecondsRemaining > 0)
        {
            SecondsRemaining--;
            ProgressPct = ((double)SecondsRemaining / _totalSecondsForCurrentMode) * 100;

            if (SecondsRemaining == 5)
            {
                IsWarningActive = true;
                _alarmService.StartFocusWarningSound();
            }
        }
        else
        {
            // Transition only after stopping to avoid duplicate ticks racing the phase change.
            _timer.Stop();
            IsRunning = false;
            StopWarning();
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            _alarmService.PlayFocusEndSound();
            TransitionToNextPhase();
        }
    }

    /// <summary>
    /// Advances pomodoro flow between focus and breaks, inserting a long break every fourth focus cycle.
    /// </summary>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: updates <see cref="CycleCount"/> and active mode state.
    /// </remarks>
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
