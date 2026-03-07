using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeeklyTimetable.Models;

namespace WeeklyTimetable.Views;

public partial class BlockDetailSheet : ContentPage
{
    /// <summary>
    /// Initializes block detail sheet and binds the detail view model for the selected block.
    /// </summary>
    /// <param name="block">Schedule block whose details are being viewed/edited.</param>
    public BlockDetailSheet(ScheduleBlock block)
    {
        InitializeComponent();
        BindingContext = new BlockDetailViewModel(block);
    }
}

public partial class BlockDetailViewModel : ObservableObject
{
    [ObservableProperty] public ScheduleBlock _block;
    [ObservableProperty] public string _notes = string.Empty;

    /// <summary>
    /// Creates a detail editor view model for one schedule block.
    /// </summary>
    /// <param name="block">Target schedule block instance.</param>
    /// <remarks>
    /// Side effects: initializes editable notes from the block.
    /// </remarks>
    public BlockDetailViewModel(ScheduleBlock block)
    {
        _block = block;
        Notes  = block.Notes ?? string.Empty;
    }

    /// <summary>
    /// Saves notes into the bound block model and dismisses the sheet.
    /// </summary>
    /// <returns>A task that completes after navigation back.</returns>
    /// <remarks>
    /// Side effects: mutates <see cref="ScheduleBlock.Notes"/> and performs shell navigation.
    /// </remarks>
    [RelayCommand]
    private async Task SaveNotesAsync()
    {
        Block.Notes = Notes;
        await Shell.Current.GoToAsync("..");
    }
}
