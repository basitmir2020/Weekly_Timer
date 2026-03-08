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
    [ObservableProperty] private ObservableCollection<HabitCommitmentViewModel> _habits = new();
    
    [ObservableProperty] private string _weekDisplay = string.Empty;
    [ObservableProperty] private WeeklyReflection _reflection = new();
    
    public List<string> Categories { get; } = new() { "work", "study", "exercise", "routine", "relax", "other" };

    private string WeekStart => GetMonday(DateTime.Today).ToString("yyyy-MM-dd");

    public GoalsViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        var monday = GetMonday(DateTime.Today);
        WeekDisplay = $"{monday:MMM dd} - {monday.AddDays(6):MMM dd}";
    }

    public Task EnsureLoadedAsync() => LoadDataAsync();

    [RelayCommand]
    private async Task AddHabitAsync()
    {
        var habit = new HabitCommitment
        {
            WeekStartDate = GetMonday(DateTime.Today).ToString("yyyy-MM-dd"),
            Title = "New Habit",
            TargetFrequency = 7
        };
        await _databaseService.SaveHabitCommitmentAsync(habit);
        Habits.Add(new HabitCommitmentViewModel(habit, _databaseService));
    }

    private async Task LoadDataAsync()
    {
        var weekStartStr = GetMonday(DateTime.Today).ToString("yyyy-MM-dd");
        WeekDisplay = $"Week of {GetMonday(DateTime.Today):MMM dd, yyyy}";

        try
        {
            var goalModels = await _databaseService.GetWeeklyGoalItemsAsync(weekStartStr);
            Goals.Clear();
            foreach (var m in goalModels)
            {
                var vm = new WeeklyGoalItemViewModel(m, _databaseService);
                await vm.LoadSubtasksAsync();
                Goals.Add(vm);
            }

            var habitModels = await _databaseService.GetHabitCommitmentsAsync(weekStartStr); // Corrected to plural
            Habits.Clear();
            foreach (var h in habitModels)
            {
                Habits.Add(new HabitCommitmentViewModel(h, _databaseService));
            }

            var reflectionModel = await _databaseService.GetWeeklyReflectionAsync(weekStartStr);
            if (reflectionModel != null)
            {
                Reflection = reflectionModel;
            }
            else
            {
                Reflection = new WeeklyReflection { WeekStartDate = weekStartStr };
                await _databaseService.SaveWeeklyReflectionAsync(Reflection);
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
