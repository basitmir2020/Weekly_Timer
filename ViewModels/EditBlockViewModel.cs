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
    [ObservableProperty] private string _selectedTime = "07:00";
    [ObservableProperty] private List<string> _availableTimes = new();
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
            TimeText        = existing.Time;
            SelectedTime    = existing.Time;
            Label           = existing.Label;
            Icon            = existing.Icon;
            Category        = existing.Category;
            DurationMinutes = existing.DurationMinutes;
        }
        else if (AvailableTimes.Any())
        {
            SelectedTime = AvailableTimes.First();
        }
    }

    private void PopulateAvailableTimes()
    {
        // Generate possible times every 30 mins
        var allTimes = Enumerable.Range(0, 48)
            .Select(i => TimeSpan.FromMinutes(i * 30).ToString(@"hh\:mm"))
            .ToList();

        // Get used times (excluding the currently edited block's time)
        var usedTimes = _currentDayBlocks
            .Where(b => _existingBlock == null || b.Time != _existingBlock.Time)
            .Select(b => b.Time)
            .ToHashSet();

        AvailableTimes = allTimes.Where(t => !usedTimes.Contains(t)).ToList();
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
        block.Time            = SelectedTime;
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
