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

    public static readonly BindableProperty TrackColorProperty = BindableProperty.Create(
        nameof(TrackColor), typeof(Color), typeof(RingProgressControl), Color.FromArgb("#1e293b"), propertyChanged: OnRingPropertyChanged);

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

    public Color TrackColor
    {
        get => (Color)GetValue(TrackColorProperty);
        set => SetValue(TrackColorProperty, value);
    }

    private readonly RingDrawable _drawable;

    public RingProgressControl()
    {
        InitializeComponent();
        
        _drawable = new RingDrawable();
        RingGraphicsView.Drawable = _drawable;

        this.Loaded += (s, e) => 
        {
            _drawable.Pct = Pct;
            _drawable.Stroke = Stroke;
            _drawable.RingColor = RingColor;
            _drawable.TrackColor = TrackColor;
            RingGraphicsView.Invalidate();
        };
    }

    private static void OnRingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is RingProgressControl control)
        {
            control._drawable.Pct = control.Pct;
            control._drawable.Stroke = control.Stroke;
            control._drawable.RingColor = control.RingColor;
            control._drawable.TrackColor = control.TrackColor;
            control.RingGraphicsView.Invalidate();
        }
    }
}

public class RingDrawable : IDrawable
{
    public double Pct { get; set; }
    public double Stroke { get; set; }
    public Color RingColor { get; set; } = Color.FromArgb("#8b5cf6");
    public Color TrackColor { get; set; } = Color.FromArgb("#1e293b");

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (dirtyRect.Width <= 0 || dirtyRect.Height <= 0) return;

        float center = dirtyRect.Width / 2;
        float radius = center - ((float)Stroke / 2);

        if (radius <= 0) return;

        canvas.StrokeColor = TrackColor;
        canvas.StrokeSize = (float)Stroke;
        canvas.DrawCircle(center, center, radius);

        if (Pct <= 0) return;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeColor = RingColor;
        
        float endAngle = (float)(90 - (Pct / 100 * 360));

        float left = ((float)Stroke / 2);
        float top = ((float)Stroke / 2);
        float width = dirtyRect.Width - (float)Stroke;
        float height = dirtyRect.Height - (float)Stroke;

        if (width <= 0 || height <= 0) return;

        canvas.DrawArc(left, top, width, height, 90, endAngle, true, false);
    }
}
