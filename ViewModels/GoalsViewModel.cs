using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeeklyTimetable.Models;
using WeeklyTimetable.Services;

namespace WeeklyTimetable.ViewModels;

public partial class GoalsViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;
    private bool _isLoaded;

    [ObservableProperty] private ObservableCollection<WeeklyGoalItemViewModel> _goals = new();
    [ObservableProperty] private ObservableCollection<HabitCommitment> _habits = new();
    
    [ObservableProperty] private string _weekDisplay = string.Empty;

    private string WeekStart => GetMonday(DateTime.Today).ToString("yyyy-MM-dd");

    public GoalsViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        var monday = GetMonday(DateTime.Today);
        WeekDisplay = $"{monday:MMM dd} - {monday.AddDays(6):MMM dd}";
    }

    public Task EnsureLoadedAsync() => LoadAsync(forceReload: false);

    private async Task LoadAsync(bool forceReload)
    {
        if (_isLoaded && !forceReload)
            return;

        try
        {
            var items = await _databaseService.GetWeeklyGoalItemsAsync(WeekStart);
            Goals.Clear();
            foreach (var item in items)
            {
                var vm = new WeeklyGoalItemViewModel(item, _databaseService);
                await vm.LoadSubtasksAsync();
                Goals.Add(vm);
            }

            var habitItems = await _databaseService.GetHabitCommitmentsAsync(WeekStart);
            Habits.Clear();
            foreach (var h in habitItems)
            {
                Habits.Add(h);
            }

            _isLoaded = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading goals: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task AddGoalAsync()
    {
        var newItem = new WeeklyGoalItem
        {
            WeekStartDate = WeekStart,
            Title = "New Goal",
            Status = GoalStatus.NotStarted,
            Priority = GoalPriority.Medium
        };
        await _databaseService.SaveWeeklyGoalItemAsync(newItem);
        var vm = new WeeklyGoalItemViewModel(newItem, _databaseService);
        Goals.Add(vm);
    }

    [RelayCommand]
    private async Task DeleteGoalAsync(WeeklyGoalItemViewModel goalVm)
    {
        if (goalVm == null) return;
        await _databaseService.DeleteWeeklyGoalItemAsync(goalVm.Model);
        Goals.Remove(goalVm);
    }

    private static DateTime GetMonday(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }
}
