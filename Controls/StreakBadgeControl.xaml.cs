using Microsoft.Maui.Controls;
using System.Windows.Input;

namespace WeeklyTimetable.Controls;

public partial class StreakBadgeControl : ContentView
{
    public static readonly BindableProperty StreakCountProperty = BindableProperty.Create(
        nameof(StreakCount),
        typeof(int),
        typeof(StreakBadgeControl),
        0);

    public int StreakCount
    {
        get => (int)GetValue(StreakCountProperty);
        set => SetValue(StreakCountProperty, value);
    }

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(StreakBadgeControl),
        null);

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public StreakBadgeControl()
    {
        InitializeComponent();
    }
}
