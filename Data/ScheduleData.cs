using WeeklyTimetable.Models;

namespace WeeklyTimetable.Data;

public static class ScheduleData
{
    /// <summary>
    /// Creates the default weekly schedule template used when no user-customized schedule exists.
    /// </summary>
    /// <returns>A dictionary keyed by day name with ordered schedule blocks.</returns>
    /// <remarks>
    /// Side effects: none; returns a newly allocated in-memory schedule.
    /// </remarks>
    public static Dictionary<string, List<ScheduleBlock>> GetDefaultSchedule()
    {
        var schedule = new Dictionary<string, List<ScheduleBlock>>();
        var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        foreach (var day in days)
        {
            // Build each day independently so callers can mutate one day without affecting others.
            schedule[day] = GetTemplateBlocksForDay(day);
        }

        return schedule;
    }

    /// <summary>
    /// Produces the default block list for a specific day and computes each block duration.
    /// </summary>
    /// <param name="day">Target day name (for example, Monday).</param>
    /// <returns>Ordered list of schedule blocks for the requested day.</returns>
    /// <remarks>
    /// Side effects: none; returns a newly allocated list.
    /// </remarks>
    private static List<ScheduleBlock> GetTemplateBlocksForDay(string day)
    {
        var isWeekend = day == "Saturday" || day == "Sunday";
        var morningStudyTopic = GetMorningStudyTopic(day);
        var eveningStudyTopic = GetEveningStudyTopic(day);

        var blocks = new List<ScheduleBlock>();

        if (!isWeekend)
        {
            blocks = new List<ScheduleBlock>
            {
                new() { Time = "05:30", Label = "Wake Up & Hydrate", Category = CategoryKeys.Routine, Icon = "🚰" },
                new() { Time = "05:45", Label = "Exercise (walk/jog or yoga)", Category = CategoryKeys.Exercise, Icon = "🏃" },
                new() { Time = "06:15", Label = "Freshen Up", Category = CategoryKeys.Routine, Icon = "🚿" },
                new() { Time = "06:30", Label = "Breakfast", Category = CategoryKeys.Meal, Icon = "🍳" },
                new() { Time = "07:00", Label = $"Deep Focus Study ({morningStudyTopic})", Category = CategoryKeys.Study, Icon = "🧠" },
                new() { Time = "08:30", Label = "Commute / Prep for Work", Category = CategoryKeys.Routine, Icon = "🚗" },
                new() { Time = "09:00", Label = "Office Work", Category = CategoryKeys.Work, Icon = "💼" },
                new() { Time = "13:00", Label = "Lunch", Category = CategoryKeys.Meal, Icon = "🥗" },
                new() { Time = "13:30", Label = "Short Walk + Rest", Category = CategoryKeys.Break, Icon = "🚶" },
                new() { Time = "14:00", Label = "Office Work (continued)", Category = CategoryKeys.Work, Icon = "💻" },
                new() { Time = "17:30", Label = "Commute Home", Category = CategoryKeys.Routine, Icon = "🏠" },
                new() { Time = "18:30", Label = "Freshen Up + Snack", Category = CategoryKeys.Meal, Icon = "🍎" },
                new() { Time = "19:00", Label = $"Deep Focus Study ({eveningStudyTopic})", Category = CategoryKeys.Study, Icon = "📚" },
                new() { Time = "20:30", Label = "Dinner", Category = CategoryKeys.Meal, Icon = "🍽️" },
                new() { Time = "21:00", Label = "Leisure / Relaxation", Category = CategoryKeys.Relax, Icon = "🛋️" },
                new() { Time = "21:30", Label = "Wind-down (Reading / Journal)", Category = CategoryKeys.Routine, Icon = "📓" },
                new() { Time = "22:00", Label = "Sleep", Category = CategoryKeys.Sleep, Icon = "💤" }
            };
        }
        else if (day == "Saturday")
        {
            blocks = new List<ScheduleBlock>
            {
                new() { Time = "07:00", Label = "Wake Up & Hydrate", Category = CategoryKeys.Routine, Icon = "🚰" },
                new() { Time = "07:30", Label = "Long Exercise (Hike/Run)", Category = CategoryKeys.Exercise, Icon = "🏃" },
                new() { Time = "09:00", Label = "Breakfast", Category = CategoryKeys.Meal, Icon = "🍳" },
                new() { Time = "10:00", Label = "Project Build + Pending DSA", Category = CategoryKeys.Study, Icon = "💻" },
                new() { Time = "13:30", Label = "Lunch", Category = CategoryKeys.Meal, Icon = "🥗" },
                new() { Time = "14:30", Label = "Leisure / Free Time", Category = CategoryKeys.Relax, Icon = "🎮" },
                new() { Time = "20:30", Label = "Dinner", Category = CategoryKeys.Meal, Icon = "🍽️" },
                new() { Time = "21:30", Label = "Wind-down", Category = CategoryKeys.Routine, Icon = "📓" },
                new() { Time = "22:00", Label = "Sleep", Category = CategoryKeys.Sleep, Icon = "💤" }
            };
        }
        else // Sunday
        {
            blocks = new List<ScheduleBlock>
            {
                new() { Time = "07:00", Label = "Wake Up & Hydrate", Category = CategoryKeys.Routine, Icon = "🚰" },
                new() { Time = "07:30", Label = "Light Exercise / Yoga", Category = CategoryKeys.Exercise, Icon = "🧘" },
                new() { Time = "08:30", Label = "Breakfast", Category = CategoryKeys.Meal, Icon = "🍳" },
                new() { Time = "09:30", Label = "DSA Revision", Category = CategoryKeys.Study, Icon = "🔁" },
                new() { Time = "11:30", Label = "Web Dev Revision", Category = CategoryKeys.Study, Icon = "🔁" },
                new() { Time = "13:30", Label = "Lunch", Category = CategoryKeys.Meal, Icon = "🥗" },
                new() { Time = "14:30", Label = "Weekly Reflection & Planning", Category = CategoryKeys.Routine, Icon = "📝" },
                new() { Time = "16:00", Label = "Leisure / Free Time", Category = CategoryKeys.Relax, Icon = "🛋️" },
                new() { Time = "20:30", Label = "Dinner", Category = CategoryKeys.Meal, Icon = "🍽️" },
                new() { Time = "21:30", Label = "Wind-down", Category = CategoryKeys.Routine, Icon = "📓" },
                new() { Time = "22:00", Label = "Sleep", Category = CategoryKeys.Sleep, Icon = "💤" }
            };
        }

        // Calculate durations assuming consecutive blocks
        for (int i = 0; i < blocks.Count - 1; i++)
        {
            var current = TimeSpan.Parse(blocks[i].Time);
            var next = TimeSpan.Parse(blocks[i + 1].Time);
            
            if (next < current) 
                next = next.Add(TimeSpan.FromDays(1)); // Handles crossing midnight

            // Duration is inferred from neighboring block start times.
            blocks[i].DurationMinutes = (int)(next - current).TotalMinutes;
        }

        // Handle last block rough estimate (e.g. 7.5 hours of sleep)
        blocks[^1].DurationMinutes = (int)TimeSpan.FromHours(7.5).TotalMinutes;

        return blocks;
    }

    /// <summary>
    /// Resolves the weekday-specific morning study topic.
    /// </summary>
    /// <param name="day">Day name used for topic selection.</param>
    /// <returns>Topic text used in the generated morning study label.</returns>
    private static string GetMorningStudyTopic(string day) => day switch
    {
        "Monday" or "Wednesday" => "DSA",
        "Tuesday" or "Thursday" => "Web Dev",
        "Friday" => "Light Review",
        _ => "Study"
    };

    /// <summary>
    /// Resolves the weekday-specific evening study topic.
    /// </summary>
    /// <param name="day">Day name used for topic selection.</param>
    /// <returns>Topic text used in the generated evening study label.</returns>
    private static string GetEveningStudyTopic(string day) => day switch
    {
        "Monday" or "Wednesday" => "Web Dev",
        "Tuesday" or "Thursday" => "DSA",
        "Friday" => "Buffer / Free",
        _ => "Study"
    };
}
