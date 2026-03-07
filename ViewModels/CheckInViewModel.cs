using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeeklyTimetable.Models;
using WeeklyTimetable.Services;

namespace WeeklyTimetable.ViewModels;

public partial class CheckInViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;

    [ObservableProperty] private int _morningEnergy = 3;
    [ObservableProperty] private int _eveningMood = 3;
    [ObservableProperty] private string _notes = string.Empty;
    [ObservableProperty] private bool _isSaved;
    [ObservableProperty] private bool _isMorning;

    /// <summary>
    /// Creates the daily check-in view model and loads any existing check-in for today.
    /// </summary>
    /// <param name="databaseService">Database service used for check-in persistence.</param>
    /// <remarks>
    /// Side effects: determines morning/evening mode and triggers asynchronous load of today's record.
    /// </remarks>
    public CheckInViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        IsMorning = DateTime.Now.Hour < 12;
        _ = LoadTodayAsync();
    }

    /// <summary>
    /// Loads today's check-in entry and hydrates the form if data already exists.
    /// </summary>
    /// <returns>A task that completes after lookup and property mapping.</returns>
    /// <remarks>
    /// Side effects: updates energy, mood, notes, and saved-state properties.
    /// </remarks>
    private async Task LoadTodayAsync()
    {
        var existing = await _databaseService.GetCheckInAsync(DateTime.Today);
        if (existing != null)
        {
            MorningEnergy = existing.MorningEnergy;
            EveningMood   = existing.EveningMood;
            Notes         = existing.Notes ?? string.Empty;
            IsSaved       = true;
        }
    }

    /// <summary>
    /// Saves today's check-in values to persistence.
    /// </summary>
    /// <returns>A task that completes when the check-in record is persisted.</returns>
    /// <remarks>
    /// Side effects: writes/updates today's <see cref="DailyCheckIn"/> and marks the form as saved.
    /// </remarks>
    [RelayCommand]
    private async Task SaveCheckInAsync()
    {
        var record = new DailyCheckIn
        {
            Date         = DateTime.Today,
            MorningEnergy = MorningEnergy,
            EveningMood  = EveningMood,
            Notes        = Notes
        };
        await _databaseService.SaveCheckInAsync(record);
        IsSaved = true;
    }
}
