using WeeklyTimetable.Models;

namespace WeeklyTimetable.Services;

public interface IDatabaseService
{
    Task InitializeAsync();

    // Generic CRUD
    Task<List<T>> GetAllAsync<T>() where T : new();
    Task<T> GetByIdAsync<T>(object id) where T : new();
    Task<int> SaveItemAsync<T>(T item) where T : new();
    Task<int> DeleteItemAsync<T>(T item) where T : new();

    // Specific helpers
    Task<DailyCheckIn?> GetCheckInAsync(DateTime date);
    Task<int> SaveCheckInAsync(DailyCheckIn entry);
    Task<List<DailyCheckIn>> GetRecentCheckInsAsync(int days);

    Task<WeeklyGoal?> GetWeeklyGoalAsync(string weekStart);
    Task<int> SaveWeeklyGoalAsync(WeeklyGoal goal);
}