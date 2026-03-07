using CommunityToolkit.Mvvm.ComponentModel;

namespace WeeklyTimetable.ViewModels;

public partial class DayOverviewViewModel : ObservableObject
{
    [ObservableProperty]
    private string _dayName = string.Empty;

    [ObservableProperty]
    private string _dayAbbreviation = string.Empty;
    
    [ObservableProperty]
    private int _totalBlocks;
    
    [ObservableProperty]
    private int _completedBlocks;
    
    [ObservableProperty]
    private int _completionPct;

    [ObservableProperty]
    private bool _isActive;
}
