namespace WeeklyTimetable.Services;

/// <summary>
/// Stub implementation of <see cref="IAlarmSoundPickerService"/> for non-Android platforms.
/// All operations are no-ops.
/// </summary>
public sealed class StubAlarmSoundPickerService : IAlarmSoundPickerService
{
    public Task<string?> PickAlarmSoundAsync() => Task.FromResult<string?>(null);
    public string GetSoundName(string? uri) => "Default";
}
