using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeeklyTimetable.Models;
using WeeklyTimetable.Services;

namespace WeeklyTimetable.ViewModels;

public partial class EditBlockViewModel : ObservableObject
{
    private readonly IDatabaseService _db;
    private readonly string _dayName;
    private readonly ScheduleBlock? _existingBlock;
    private readonly List<ScheduleBlock> _currentDayBlocks;
    
    public bool IsEditing => _existingBlock != null;

    [ObservableProperty] private string _timeText = "07:00";
    [ObservableProperty] private TimeOption? _selectedTimeOption;
    [ObservableProperty] private List<TimeOption> _availableTimes = new();
    [ObservableProperty] private string _label = string.Empty;
    [ObservableProperty] private string _icon = "📌";
    [ObservableProperty] private string _category = "routine";
    [ObservableProperty] private int _durationMinutes = 30;

    public List<string> Categories { get; } = new()
    {
        "sleep", "work", "study", "exercise", "meal", "break", "relax", "routine"
    };

    // Callback so MainViewModel can refresh its list
    public Action<ScheduleBlock, bool>? OnSaved;  // (block, isNew)

    public EditBlockViewModel(IDatabaseService db, string dayName, ScheduleBlock? existing = null, List<ScheduleBlock>? currentDayBlocks = null)
    {
        _db = db;
        _dayName = dayName;
        _existingBlock = existing;
        _currentDayBlocks = currentDayBlocks ?? new List<ScheduleBlock>();

        PopulateAvailableTimes();

        if (existing != null)
        {
            TimeText           = existing.Time;
            SelectedTimeOption = AvailableTimes.FirstOrDefault(t => t.Time24 == existing.Time) ?? new TimeOption { Time24 = existing.Time, DisplayTime = existing.Time };
            Label              = existing.Label;
            Icon               = existing.Icon;
            Category           = existing.Category;
            DurationMinutes    = existing.DurationMinutes;
        }
        else if (AvailableTimes.Any())
        {
            SelectedTimeOption = AvailableTimes.First();
        }
    }

    private void PopulateAvailableTimes()
    {
        var existingTimes = _currentDayBlocks
            .Select(b => b.Time)
            .Where(t => TimeSpan.TryParse(t, out _))
            .Distinct()
            .Select(t => TimeSpan.Parse(t))
            .OrderBy(t => t)
            .ToList();

        var options = new List<TimeOption>();
        foreach (var ts in existingTimes)
        {
            var dt = DateTime.Today.Add(ts);
            string display = dt.ToString("h:mm tt").ToLower(); // matches "5:30 am"
            options.Add(new TimeOption { Time24 = ts.ToString(@"hh\:mm"), DisplayTime = display });
        }
        
        AvailableTimes = options;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Label))
        {
            await Shell.Current.DisplayAlert("Validation", "Label cannot be empty.", "OK");
            return;
        }

        var block = _existingBlock ?? new ScheduleBlock();
        block.Time            = SelectedTimeOption?.Time24 ?? "07:00";
        block.Label           = Label;
        block.Icon            = Icon;
        block.Category        = Category;
        block.DurationMinutes = DurationMinutes;

        bool isNew = _existingBlock == null;
        OnSaved?.Invoke(block, isNew);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (_existingBlock == null) return;

        bool confirm = await Shell.Current.DisplayAlert(
            "Delete Block", $"Delete \"{_existingBlock.Label}\"?", "Delete", "Cancel");
        if (confirm)
        {
            OnSaved?.Invoke(_existingBlock, false); // Signal deletion via negative flag
            await Shell.Current.GoToAsync("..");
        }
    }
}

public class TimeOption
{
    public string Time24 { get; set; } = string.Empty;
    public string DisplayTime { get; set; } = string.Empty;
}
