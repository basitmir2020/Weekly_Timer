using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeeklyTimetable.Models;
using WeeklyTimetable.Services;

namespace WeeklyTimetable.ViewModels;

public partial class HabitCommitmentViewModel : ObservableObject
{
    private readonly HabitCommitment _model;
    private readonly IDatabaseService _databaseService;

    public HabitCommitment Model => _model;

    [ObservableProperty] private string _title;
    [ObservableProperty] private int _targetFrequency;
    [ObservableProperty] private bool[] _dailyChecks = new bool[7];

    public HabitCommitmentViewModel(HabitCommitment model, IDatabaseService databaseService)
    {
        _model = model;
        _databaseService = databaseService;
        _title = model.Title;
        _targetFrequency = model.TargetFrequency;
        
        LoadDailyChecks(model.DailyChecks);
    }

    private void LoadDailyChecks(string checks)
    {
        var parts = checks.Split(',');
        for (int i = 0; i < Math.Min(parts.Length, 7); i++)
        {
            _dailyChecks[i] = parts[i] == "1";
        }
    }

    private string GetDailyChecksString()
    {
        return string.Join(",", _dailyChecks.Select(c => c ? "1" : "0"));
    }

    [RelayCommand]
    private async Task ToggleDayAsync(string dayIndexStr)
    {
        if (int.TryParse(dayIndexStr, out int index) && index >= 0 && index < 7)
        {
            _dailyChecks[index] = !_dailyChecks[index];
            _model.DailyChecks = GetDailyChecksString();
            _model.CompletedCount = _dailyChecks.Count(c => c);
            
            await _databaseService.SaveHabitCommitmentAsync(_model);
            OnPropertyChanged(nameof(DailyChecks));
            // Trigger refresh of individual day properties if needed for UI
            OnPropertyChanged($"Day{index}Checked");
        }
    }

    public bool Day0Checked => _dailyChecks[0];
    public bool Day1Checked => _dailyChecks[1];
    public bool Day2Checked => _dailyChecks[2];
    public bool Day3Checked => _dailyChecks[3];
    public bool Day4Checked => _dailyChecks[4];
    public bool Day5Checked => _dailyChecks[5];
    public bool Day6Checked => _dailyChecks[6];

    partial void OnTitleChanged(string value) { _model.Title = value; Save(); }
    partial void OnTargetFrequencyChanged(int value) { _model.TargetFrequency = value; Save(); }

    private void Save()
    {
        _ = _databaseService.SaveHabitCommitmentAsync(_model);
    }
}
