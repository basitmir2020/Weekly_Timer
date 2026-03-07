namespace WeeklyTimetable.Controls;

public partial class RingProgressControl : ContentView
{
    public static readonly BindableProperty PctProperty = BindableProperty.Create(
        nameof(Pct), typeof(double), typeof(RingProgressControl), 0.0, propertyChanged: OnRingPropertyChanged);

    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(double), typeof(RingProgressControl), 52.0, propertyChanged: OnRingPropertyChanged);

    public static readonly BindableProperty StrokeProperty = BindableProperty.Create(
        nameof(Stroke), typeof(double), typeof(RingProgressControl), 5.0, propertyChanged: OnRingPropertyChanged);

    public static readonly BindableProperty RingColorProperty = BindableProperty.Create(
        nameof(RingColor), typeof(Color), typeof(RingProgressControl), Color.FromArgb("#8b5cf6"), propertyChanged: OnRingPropertyChanged);

    public double Pct
    {
        get => (double)GetValue(PctProperty);
        set => SetValue(PctProperty, value);
    }

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public double Stroke
    {
        get => (double)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public Color RingColor
    {
        get => (Color)GetValue(RingColorProperty);
        set => SetValue(RingColorProperty, value);
    }

    private readonly RingDrawable _drawable;

    /// <summary>
    /// Initializes ring progress control and binds the drawable renderer to the graphics view.
    /// </summary>
    /// <remarks>
    /// Side effects: attaches a loaded event to force initial rendering.
    /// </remarks>
    public RingProgressControl()
    {
        InitializeComponent();
        
        _drawable = new RingDrawable();
        RingGraphicsView.Drawable = _drawable;

        // Ensure the initial properties are rendered by Skia when the control loads, solving the blank canvas bug
        this.Loaded += (s, e) => 
        {
            _drawable.Pct = Pct;
            _drawable.Stroke = Stroke;
            _drawable.RingColor = RingColor;
            RingGraphicsView.Invalidate();
        };
    }

    /// <summary>
    /// Applies bindable property changes to the drawable and requests a redraw.
    /// </summary>
    /// <param name="bindable">Control whose ring property changed.</param>
    /// <param name="oldValue">Previous value.</param>
    /// <param name="newValue">New value.</param>
    /// <returns>None.</returns>
    private static void OnRingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is RingProgressControl control)
        {
            control._drawable.Pct = control.Pct;
            control._drawable.Stroke = control.Stroke;
            control._drawable.RingColor = control.RingColor;
            control.RingGraphicsView.Invalidate();
        }
    }
}

public class RingDrawable : IDrawable
{
    public double Pct { get; set; }
    public double Stroke { get; set; }
    public Color RingColor { get; set; } = Color.FromArgb("#8b5cf6");

    /// <summary>
    /// Draws a circular progress track and arc based on the current percentage.
    /// </summary>
    /// <param name="canvas">Drawing canvas supplied by MAUI.</param>
    /// <param name="dirtyRect">Bounds for the drawable area.</param>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: renders vector primitives on the provided canvas.
    /// </remarks>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float center = dirtyRect.Width / 2;
        float radius = center - ((float)Stroke / 2);

        // Draw background track
        canvas.StrokeColor = Color.FromArgb("#1e293b"); // BorderVisible
        canvas.StrokeSize = (float)Stroke;
        canvas.DrawCircle(center, center, radius);

        if (Pct <= 0) return;

        // Draw progress arc
        canvas.StrokeColor = RingColor;
        canvas.StrokeSize = (float)Stroke;
        canvas.StrokeLineCap = LineCap.Round;
        
        float endAngle = (float)(90 - (Pct / 100 * 360));

        // MAUI DrawArc logic (angle is 0 at right, 90 at top, 180 at left, -90 at bottom)
        // DrawArc bounds 
        // Explicit bounds prevent stroke clipping at the control edges.
        float left = dirtyRect.X + ((float)Stroke / 2);
        float top = dirtyRect.Y + ((float)Stroke / 2);
        float width = dirtyRect.Width - (float)Stroke;
        float height = dirtyRect.Height - (float)Stroke;

        canvas.DrawArc(left, top, width, height, 90, endAngle, true, false);
    }
}
