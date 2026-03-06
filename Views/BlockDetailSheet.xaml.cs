using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeeklyTimetable.Models;
using WeeklyTimetable.Services;

namespace WeeklyTimetable.Views;

public partial class BlockDetailSheet : ContentPage
{
    public BlockDetailSheet(ScheduleBlock block, IDatabaseService db)
    {
        InitializeComponent();
        BindingContext = new BlockDetailViewModel(block, db);
    }
}

public partial class BlockDetailViewModel : ObservableObject
{
    private readonly IDatabaseService _db;

    [ObservableProperty] public ScheduleBlock _block;
    [ObservableProperty] public string _notes = string.Empty;

    public BlockDetailViewModel(ScheduleBlock block, IDatabaseService db)
    {
        _block = block;
        _db    = db;
        Notes  = block.Notes ?? string.Empty;
    }

    [RelayCommand]
    private async Task SaveNotesAsync()
    {
        Block.Notes = Notes;
        await Shell.Current.GoToAsync("..");
    }
}
