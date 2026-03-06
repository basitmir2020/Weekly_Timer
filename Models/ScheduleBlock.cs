using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace WeeklyTimetable.Models;

public partial class ScheduleBlock : ObservableObject
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Time { get; set; }          // "HH:MM" 24-hr stored; displayed as 12-hr
    public string Label { get; set; }         // Human-readable description of the activity
    public string Category { get; set; }      // One of 8 category keys (e.g., "work", "study")
    public string Icon { get; set; }          // Emoji 
    
    [ObservableProperty]
    private bool _isCompleted;                // Bound to checkbox / tap gesture
    
    public string? Notes { get; set; }        // Optional per-block annotation
    public int DurationMinutes { get; set; }  // Computed from consecutive block timestamps
}
