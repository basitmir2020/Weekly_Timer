using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace WeeklyTimetable.Controls;

public partial class EnergySliderControl : ContentView
{
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(int),
        typeof(EnergySliderControl),
        0,
        BindingMode.TwoWay,
        propertyChanged: OnValueChanged);

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Initializes the energy slider control and builds selectable rating dots.
    /// </summary>
    /// <remarks>
    /// Side effects: populates visual child elements in <c>DotsContainer</c>.
    /// </remarks>
    public EnergySliderControl()
    {
        InitializeComponent();
        GenerateDots();
    }

    /// <summary>
    /// Creates the five selectable dot UI elements and wires tap handlers for two-way value updates.
    /// </summary>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: clears and repopulates <c>DotsContainer</c>, and attaches gesture handlers.
    /// </remarks>
    private void GenerateDots()
    {
        DotsContainer.Children.Clear();
        for (int i = 1; i <= 5; i++)
        {
            var border = new Border
            {
                WidthRequest = 40,
                HeightRequest = 40,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(20) },
                StrokeThickness = 2,
                BackgroundColor = Color.Parse("#09090f"), // BgCardDim
                Stroke = Color.Parse("#1e293b"), // BorderVisible
                Content = new Label 
                { 
                    Text = i.ToString(), 
                    HorizontalOptions = LayoutOptions.Center, 
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Color.Parse("#334155") // TextMuted
                }
            };

            var tapGesture = new TapGestureRecognizer();
            int selectedValue = i;
            tapGesture.Tapped += (s, e) =>
            {
                // Capture loop variable via local copy to avoid closure issues across iterations.
                Value = selectedValue;
            };
            border.GestureRecognizers.Add(tapGesture);

            SemanticProperties.SetDescription(border, $"Rating {i} out of 5");
            SemanticProperties.SetHint(border, "Double tap to select this rating value");

            DotsContainer.Children.Add(border);
        }
        UpdateDots();
    }

    /// <summary>
    /// Handles bindable <see cref="Value"/> changes and refreshes dot visuals.
    /// </summary>
    /// <param name="bindable">Control instance whose value changed.</param>
    /// <param name="oldValue">Previous value.</param>
    /// <param name="newValue">New value.</param>
    /// <returns>None.</returns>
    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        (bindable as EnergySliderControl)?.UpdateDots();
    }

    /// <summary>
    /// Applies active/inactive styles across all dots based on current selected value.
    /// </summary>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: updates border/background/text colors of child dot elements.
    /// </remarks>
    private void UpdateDots()
    {
        if (DotsContainer == null || DotsContainer.Children.Count == 0) return;

        Color activeColor = GetColorForValue(Value);

        for (int i = 0; i < 5; i++)
        {
            var border = DotsContainer.Children[i] as Border;
            if (border != null)
            {
                var label = border.Content as Label;
                int dotValue = i + 1;

                if (dotValue <= Value)
                {
                    // All dots up to selected value are highlighted to create a progressive rating effect.
                    border.BackgroundColor = activeColor.WithAlpha(0.2f);
                    border.Stroke = activeColor;
                    if (label != null) label.TextColor = activeColor;
                }
                else
                {
                    border.BackgroundColor = Color.Parse("#09090f");
                    border.Stroke = Color.Parse("#1e293b");
                    if (label != null) label.TextColor = Color.Parse("#334155");
                }
            }
        }
    }

    /// <summary>
    /// Maps a selected numeric rating to a UI accent color.
    /// </summary>
    /// <param name="val">Energy rating from 1 to 5.</param>
    /// <returns>Color associated with the selected rating.</returns>
    private Color GetColorForValue(int val)
    {
        return val switch
        {
            1 => Color.Parse("#3b82f6"), // Blue (Sleep) or map to energy colors
            2 => Color.Parse("#14b8a6"), // Teal
            3 => Color.Parse("#f59e0b"), // Yellow/Amber
            4 => Color.Parse("#f97316"), // Orange
            5 => Color.Parse("#22c55e"), // Green (Work/Success)
            _ => Color.Parse("#818cf8")  // Purple
        };
    }
}
