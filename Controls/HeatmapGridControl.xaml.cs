using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Controls;

public partial class HeatmapGridControl : ContentView
{
    private readonly NotifyCollectionChangedEventHandler _collectionChangedHandler;
    private INotifyCollectionChanged? _subscribedCollection;

    public static readonly BindableProperty HeatmapCellsProperty = BindableProperty.Create(
        nameof(HeatmapCells),
        typeof(ObservableCollection<HeatmapCell>),
        typeof(HeatmapGridControl),
        null,
        propertyChanged: OnHeatmapDataChanged);

    public ObservableCollection<HeatmapCell> HeatmapCells
    {
        get => (ObservableCollection<HeatmapCell>)GetValue(HeatmapCellsProperty);
        set => SetValue(HeatmapCellsProperty, value);
    }

    /// <summary>
    /// Initializes the heatmap control.
    /// </summary>
    public HeatmapGridControl()
    {
        InitializeComponent();
        _collectionChangedHandler = (_, _) => canvasView.InvalidateSurface();
    }

    /// <summary>
    /// Rebinds collection change notifications and invalidates the canvas when heatmap data changes.
    /// </summary>
    /// <param name="bindable">Control instance receiving new data.</param>
    /// <param name="oldValue">Previous heatmap collection.</param>
    /// <param name="newValue">New heatmap collection.</param>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: subscribes to collection change events and triggers redraw requests.
    /// </remarks>
    private static void OnHeatmapDataChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not HeatmapGridControl control)
            return;

        if (control._subscribedCollection != null)
            control._subscribedCollection.CollectionChanged -= control._collectionChangedHandler;

        control._subscribedCollection = newValue as INotifyCollectionChanged;

        if (control._subscribedCollection != null)
            control._subscribedCollection.CollectionChanged += control._collectionChangedHandler;

        control.canvasView.InvalidateSurface();
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        if (args.NewHandler == null && _subscribedCollection != null)
        {
            _subscribedCollection.CollectionChanged -= _collectionChangedHandler;
            _subscribedCollection = null;
        }

        base.OnHandlerChanging(args);
    }

    /// <summary>
    /// Draws a 12x7 contribution-style heatmap grid from bound cell data.
    /// </summary>
    /// <param name="sender">Canvas view source.</param>
    /// <param name="e">Skia paint event args containing surface and dimensions.</param>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: paints heatmap cells on the Skia canvas.
    /// </remarks>
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var info = e.Info;
        var surface = e.Surface;
        var canvas = surface.Canvas;
        
        canvas.Clear();

        if (HeatmapCells == null || HeatmapCells.Count == 0)
            return;

        int columns = 12; // weeks
        int rows = 7;     // days
        
        float density = (float)Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Density;
        if (density <= 0) density = 1;

        float margin = 4 * density;
        float cellSize = (info.Height - (rows * margin) - margin) / rows;
        if (cellSize > 24 * density) cellSize = 24 * density;
        
        // Right-align the grid so it remains visually balanced on wider cards.
        float startX = info.Width - (columns * (cellSize + margin));
        if (startX < 0) startX = margin;
        
        using var paint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };
        
        int dataIndex = 0;
        
        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                float x = startX + c * (cellSize + margin);
                float y = margin + r * (cellSize + margin);
                
                string hexColor = "#0d1117"; // Empty/Dark fallback
                if (dataIndex < HeatmapCells.Count)
                {
                    hexColor = HeatmapCells[dataIndex].CellColor;
                    dataIndex++;
                }

                if (SKColor.TryParse(hexColor, out SKColor color))
                {
                    paint.Color = color;
                }
                else
                {
                    // Invalid color payloads are safely replaced with fallback to avoid render exceptions.
                    paint.Color = SKColor.Parse("#0d1117");
                }
                
                var rect = new SKRect(x, y, x + cellSize, y + cellSize);
                canvas.DrawRoundRect(rect, 4 * density, 4 * density, paint);
            }
        }
    }
}
