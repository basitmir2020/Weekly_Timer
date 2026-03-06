using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Controls;

public partial class HeatmapGridControl : ContentView
{
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

    public HeatmapGridControl()
    {
        InitializeComponent();
    }

    private static void OnHeatmapDataChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= (s, e) => (bindable as HeatmapGridControl)?.canvasView.InvalidateSurface();
            collection.CollectionChanged += (s, e) => (bindable as HeatmapGridControl)?.canvasView.InvalidateSurface();
        }
        (bindable as HeatmapGridControl)?.canvasView.InvalidateSurface();
    }

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
                    paint.Color = SKColor.Parse("#0d1117");
                }
                
                var rect = new SKRect(x, y, x + cellSize, y + cellSize);
                canvas.DrawRoundRect(rect, 4 * density, 4 * density, paint);
            }
        }
    }
}
