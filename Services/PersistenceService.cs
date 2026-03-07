using System.Text.Json;

namespace WeeklyTimetable.Services;

public class PersistenceService : IPersistenceService
{
    public async Task SaveStateAsync<T>(string key, T data)
    {
        try
        {
            await Task.Run(() => 
            {
                var json = JsonSerializer.Serialize(data);
                Preferences.Default.Set(key, json);
            });
        }
        catch (Exception ex)
        {
            // Log or handle serialization/storage errors
            Console.WriteLine($"Error saving state for key {key}: {ex.Message}");
        }
    }

    public async Task<T?> LoadStateAsync<T>(string key)
    {
        try
        {
            return await Task.Run(() => 
            {
                var json = Preferences.Default.Get<string?>(key, null);
                if (string.IsNullOrEmpty(json))
                    return default(T);

                return JsonSerializer.Deserialize<T>(json);
            });
        }
        catch (Exception ex)
        {
            // Log or handle deserialization errors (e.g., model changed)
            Console.WriteLine($"Error loading state for key {key}: {ex.Message}");
            return default(T);
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
