using System.Text.Json;

namespace WeeklyTimetable.Services;

public class PersistenceService : IPersistenceService
{
    public Task SaveStateAsync<T>(string key, T data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data);
            Preferences.Default.Set(key, json);
        }
        catch (Exception ex)
        {
            // Log or handle serialization/storage errors
            Console.WriteLine($"Error saving state for key {key}: {ex.Message}");
        }
        
        return Task.CompletedTask;
    }

    public Task<T?> LoadStateAsync<T>(string key)
    {
        try
        {
            var json = Preferences.Default.Get<string?>(key, null);
            if (string.IsNullOrEmpty(json))
                return Task.FromResult<T?>(default);

            var data = JsonSerializer.Deserialize<T>(json);
            return Task.FromResult(data);
        }
        catch (Exception ex)
        {
            // Log or handle deserialization errors (e.g., model changed)
            Console.WriteLine($"Error loading state for key {key}: {ex.Message}");
            return Task.FromResult<T?>(default);
        }
    }

    public void RemoveState(string key)
    {
        Preferences.Default.Remove(key);
    }

    public void ClearAllState()
    {
        Preferences.Default.Clear();
    }
}
