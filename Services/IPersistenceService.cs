namespace WeeklyTimetable.Services;

public interface IPersistenceService
{
    Task SaveStateAsync<T>(string key, T data);
    Task<T?> LoadStateAsync<T>(string key);
    void RemoveState(string key);
    void ClearAllState();
}
