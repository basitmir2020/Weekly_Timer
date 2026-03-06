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

    public CheckInViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        IsMorning = DateTime.Now.Hour < 12;
        _ = LoadTodayAsync();
    }

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
