using CommunityToolkit.Mvvm.ComponentModel;

namespace WeeklyTimetable.ViewModels;

public partial class DayOverviewViewModel : ObservableObject
{
    [ObservableProperty]
    private string _dayName;

    [ObservableProperty]
    private string _dayAbbreviation;
    
    [ObservableProperty]
    private int _totalBlocks;
    
    [ObservableProperty]
    private int _completedBlocks;
    
    [ObservableProperty]
    private int _completionPct;

    [ObservableProperty]
    private bool _isActive;
}
