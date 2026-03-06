using WeeklyTimetable.Models;

namespace WeeklyTimetable.Services;

public interface IProfileService
{
    Task<List<ScheduleProfile>> GetAllProfilesAsync();
    Task ActivateProfileAsync(string profileId);
    Task DeleteProfileAsync(string profileId);
    Task<ScheduleProfile?> GetActiveProfileAsync();
}

public class ProfileService : IProfileService
{
    private const string ACTIVE_KEY = "active_profile";

    private readonly List<ScheduleProfile> _builtInProfiles = new()
    {
        new ScheduleProfile { Id = "default",  Name = "Default",  IsActive = true },
        new ScheduleProfile { Id = "travel",   Name = "Travel",   IsActive = false },
        new ScheduleProfile { Id = "sick_day", Name = "Sick Day", IsActive = false },
        new ScheduleProfile { Id = "exam_mode",Name = "Exam Mode",IsActive = false },
    };

    public Task<List<ScheduleProfile>> GetAllProfilesAsync()
    {
        var activeId = Preferences.Get(ACTIVE_KEY, "default");
        var list = _builtInProfiles.ToList();
        foreach (var p in list) p.IsActive = p.Id == activeId;
        return Task.FromResult(list);
    }

    public Task ActivateProfileAsync(string profileId)
    {
        Preferences.Set(ACTIVE_KEY, profileId);
        return Task.CompletedTask;
    }

    public Task DeleteProfileAsync(string profileId)
    {
        // Built-in profiles cannot be deleted; custom ones would be stored in SQLite
        return Task.CompletedTask;
    }

    public async Task<ScheduleProfile?> GetActiveProfileAsync()
    {
        var all = await GetAllProfilesAsync();
        return all.FirstOrDefault(p => p.IsActive);
    }
}
