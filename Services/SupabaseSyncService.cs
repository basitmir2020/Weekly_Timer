using Supabase;
using Microsoft.Maui.Storage;
using System.Text.Json;
using System.Threading.Tasks;
using WeeklyTimetable.Models;

namespace WeeklyTimetable.Services;

public interface ISupabaseSyncService
{
    Task InitializeAsync(string url, string key);
    Task<bool> SignInAsync(string email, string password);
    Task<bool> BackupDataAsync();
    Task<bool> RestoreDataAsync();
}

public class SupabaseSyncService : ISupabaseSyncService
{
    private Client? _client;
    private readonly IDatabaseService _db;

    public SupabaseSyncService(IDatabaseService db)
    {
        _db = db;
    }

    public async Task InitializeAsync(string url, string key)
    {
        var options = new SupabaseOptions { AutoConnectRealtime = true };
        _client = new Client(url, key, options);
        await _client.InitializeAsync();
    }

    public async Task<bool> SignInAsync(string email, string password)
    {
        if (_client == null) return false;
        try
        {
            var session = await _client.Auth.SignIn(email, password);
            return session?.User != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> BackupDataAsync()
    {
        if (_client?.Auth.CurrentUser == null) return false;

        try
        {
            // Serialize local preferences (sched_v2) 
            string schedData = Preferences.Get("sched_v2", "{}");

            // Assuming a table 'user_backups' with columns 'user_id', 'sched_json'
            var backup = new UserBackup
            {
                UserId = _client.Auth.CurrentUser.Id,
                SchedJson = schedData,
                UpdatedAt = DateTime.UtcNow
            };

            await _client.From<UserBackup>().Upsert(backup);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RestoreDataAsync()
    {
        if (_client?.Auth.CurrentUser == null) return false;

        try
        {
            var response = await _client.From<UserBackup>()
                                        .Where(b => b.UserId == _client.Auth.CurrentUser.Id)
                                        .Single();

            if (response != null && !string.IsNullOrEmpty(response.SchedJson))
            {
                Preferences.Set("sched_v2", response.SchedJson);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
}

[Postgrest.Attributes.Table("user_backups")]
public class UserBackup : Postgrest.Models.BaseModel
{
    [Postgrest.Attributes.PrimaryKey("user_id", false)]
    public string UserId { get; set; } = string.Empty;

    [Postgrest.Attributes.Column("sched_json")]
    public string SchedJson { get; set; } = string.Empty;

    [Postgrest.Attributes.Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
