using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeeklyTimetable.Models;
using WeeklyTimetable.Services;
using System.Collections.ObjectModel;

namespace WeeklyTimetable.ViewModels;

public partial class WeeklyGoalItemViewModel : ObservableObject
{
    private readonly WeeklyGoalItem _model;
    private readonly IDatabaseService _databaseService;

    public WeeklyGoalItem Model => _model;

    [ObservableProperty] private string _title;
    [ObservableProperty] private string _description;
    [ObservableProperty] private string _category;
    [ObservableProperty] private GoalStatus _status;
    [ObservableProperty] private GoalPriority _priority;
    [ObservableProperty] private double _progressPercent;
    
    public ObservableCollection<GoalSubtaskViewModel> Subtasks { get; } = new();

    public WeeklyGoalItemViewModel(WeeklyGoalItem model, IDatabaseService databaseService)
    {
        _model = model;
        _databaseService = databaseService;
        
        _title = model.Title;
        _description = model.Description;
        _category = model.Category;
        _status = model.Status;
        _priority = model.Priority;
        _progressPercent = model.ProgressPercent;
    }

    partial void OnTitleChanged(string value) { _model.Title = value; SaveCommand.Execute(null); }
    partial void OnDescriptionChanged(string value) { _model.Description = value; SaveCommand.Execute(null); }
    partial void OnCategoryChanged(string value) { _model.Category = value; SaveCommand.Execute(null); }
    partial void OnPriorityChanged(GoalPriority value) { _model.Priority = value; SaveCommand.Execute(null); }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await _databaseService.SaveWeeklyGoalItemAsync(_model);
    }

    [RelayCommand]
    private async Task ToggleStatusAsync()
    {
        Status = Status == GoalStatus.Completed ? GoalStatus.InProgress : GoalStatus.Completed;
        _model.Status = Status;
        if (Status == GoalStatus.Completed)
        {
            _model.CompletedAt = DateTime.Now;
            _model.ProgressPercent = 100;
            ProgressPercent = 100;
        }
        else
        {
            _model.CompletedAt = null;
        }
        await _databaseService.SaveWeeklyGoalItemAsync(_model);
    }

    [RelayCommand]
    private async Task AddSubtaskAsync()
    {
        var subtask = new GoalSubtask
        {
            GoalItemId = _model.Id,
            Title = "New Subtask",
            IsCompleted = false
        };
        await _databaseService.SaveGoalSubtaskAsync(subtask);
        Subtasks.Add(new GoalSubtaskViewModel(subtask, _databaseService, this));
        UpdateProgressFromSubtasks();
        await _databaseService.SaveWeeklyGoalItemAsync(_model);
    }

    [RelayCommand]
    private async Task DeleteSubtaskAsync(GoalSubtaskViewModel subtaskVm)
    {
        if (subtaskVm == null) return;
        await _databaseService.DeleteGoalSubtaskAsync(subtaskVm.Model);
        Subtasks.Remove(subtaskVm);
        UpdateProgressFromSubtasks();
        await _databaseService.SaveWeeklyGoalItemAsync(_model);
    }

    public async Task LoadSubtasksAsync()
    {
        var subtasks = await _databaseService.GetGoalSubtasksAsync(_model.Id);
        Subtasks.Clear();
        foreach (var s in subtasks)
        {
            Subtasks.Add(new GoalSubtaskViewModel(s, _databaseService, this));
        }
        UpdateProgressFromSubtasks();
    }

    public void UpdateProgressFromSubtasks()
    {
        if (Subtasks.Count == 0) return;
        
        int completed = Subtasks.Count(s => s.IsCompleted);
        ProgressPercent = (double)completed / Subtasks.Count * 100;
        _model.ProgressPercent = ProgressPercent;
    }
}

public partial class GoalSubtaskViewModel : ObservableObject
{
    private readonly GoalSubtask _model;
    private readonly IDatabaseService _databaseService;
    private readonly WeeklyGoalItemViewModel _parent;

    [ObservableProperty] private string _title;
    [ObservableProperty] private bool _isCompleted;

    public GoalSubtaskViewModel(GoalSubtask model, IDatabaseService databaseService, WeeklyGoalItemViewModel parent)
    {
        _model = model;
        _databaseService = databaseService;
        _parent = parent;
        _title = model.Title;
        _isCompleted = model.IsCompleted;
    }

    [RelayCommand]
    private async Task ToggleAsync()
    {
        IsCompleted = !IsCompleted;
        _model.IsCompleted = IsCompleted;
        _model.CompletedAt = IsCompleted ? DateTime.Now : null;
        await _databaseService.SaveGoalSubtaskAsync(_model);
        _parent.UpdateProgressFromSubtasks();
        await _databaseService.SaveWeeklyGoalItemAsync(_parent.Model);
    }
}
