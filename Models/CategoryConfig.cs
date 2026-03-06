namespace WeeklyTimetable.Models;

public class CategoryConfig
{
    public string Key { get; set; }
    public string Label { get; set; }
    public string AccentColor { get; set; }
}

public static class CategoryKeys
{
    public const string Sleep = "sleep";
    public const string Work = "work";
    public const string Study = "study";
    public const string Exercise = "exercise";
    public const string Meal = "meal";
    public const string Break = "break";
    public const string Relax = "relax";
    public const string Routine = "routine";
}
