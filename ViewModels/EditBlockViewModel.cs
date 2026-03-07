using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeeklyTimetable.Models;

namespace WeeklyTimetable.ViewModels;

public partial class EditBlockViewModel : ObservableObject
{
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
    [ObservableProperty] private string _notes = string.Empty;

    public List<string> Categories { get; } = new()
    {
        "sleep", "work", "study", "exercise", "meal", "break", "relax", "routine"
    };

    // Callback so MainViewModel can refresh its list
    public Action<ScheduleBlock, bool>? OnSaved;  // (block, isNew)

    /// <summary>
    /// Creates a view model for adding or editing a schedule block for a specific day.
    /// </summary>
    /// <param name="existing">Existing block when editing; <c>null</c> for create mode.</param>
    /// <param name="currentDayBlocks">Current day block list used to build time options.</param>
    /// <remarks>
    /// Side effects: initializes editable properties and prepopulates selectable time options.
    /// </remarks>
    public EditBlockViewModel(ScheduleBlock? existing = null, List<ScheduleBlock>? currentDayBlocks = null)
    {
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
            Notes              = existing.Notes ?? string.Empty;
        }
        else if (AvailableTimes.Any())
        {
            SelectedTimeOption = AvailableTimes.First();
        }
    }

    /// <summary>
    /// Builds available time options from existing blocks so users can align new blocks to known schedule slots.
    /// </summary>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: replaces <see cref="AvailableTimes"/> with sorted, distinct entries.
    /// </remarks>
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
            // Use lowercase AM/PM text to match current design language.
            var dt = DateTime.Today.Add(ts);
            string display = dt.ToString("h:mm tt").ToLower(); // matches "5:30 am"
            options.Add(new TimeOption { Time24 = ts.ToString(@"hh\:mm"), DisplayTime = display });
        }
        
        AvailableTimes = options;
    }

    /// <summary>
    /// Validates and saves the edited block, then notifies the owner view model through callback.
    /// </summary>
    /// <returns>A task that completes after toast display and back navigation.</returns>
    /// <remarks>
    /// Side effects: mutates or creates a block instance, invokes <see cref="OnSaved"/>, shows toast, and navigates back.
    /// </remarks>
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
        block.Notes           = Notes;

        bool isNew = _existingBlock == null;
        OnSaved?.Invoke(block, isNew);
        
        await Toast.Make("Block saved successfully!", ToastDuration.Short).Show();
        
        await Shell.Current.GoToAsync("..");
    }

    /// <summary>
    /// Confirms and requests deletion of the existing block via callback to the owning screen.
    /// </summary>
    /// <returns>A task that completes after confirmation and navigation.</returns>
    /// <remarks>
    /// Side effects: may invoke <see cref="OnSaved"/> as a delete signal and navigate back.
    /// </remarks>
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
