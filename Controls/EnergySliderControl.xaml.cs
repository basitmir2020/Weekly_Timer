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

    public EnergySliderControl()
    {
        InitializeComponent();
        GenerateDots();
    }

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
                Value = selectedValue;
            };
            border.GestureRecognizers.Add(tapGesture);

            SemanticProperties.SetDescription(border, $"Rating {i} out of 5");
            SemanticProperties.SetHint(border, "Double tap to select this rating value");

            DotsContainer.Children.Add(border);
        }
        UpdateDots();
    }

    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        (bindable as EnergySliderControl)?.UpdateDots();
    }

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
