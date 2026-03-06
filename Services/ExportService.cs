using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace WeeklyTimetable.Services;

public interface IExportService
{
    Task<string?> ExportWeeklySummaryAsPngAsync();
}

public class ExportService : IExportService
{
    public async Task<string?> ExportWeeklySummaryAsPngAsync()
    {
        try
        {
            // Build a 1080×600 summary card at 2x DPI
            int width  = 1080;
            int height = 600;

            using var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;

            // Background
            canvas.Clear(SKColor.Parse("#07090f"));

            // Title
            using var titlePaint = new SKPaint
            {
                Color       = SKColor.Parse("#f1f5f9"),
                TextSize    = 48,
                IsAntialias = true,
                FakeBoldText = true
            };
            canvas.DrawText("Weekly Blueprint", 48, 72, titlePaint);

            // Sub-title
            using var subPaint = new SKPaint
            {
                Color    = SKColor.Parse("#818cf8"),
                TextSize = 28,
                IsAntialias = true
            };
            canvas.DrawText("Weekly Summary · " + DateTime.Now.ToString("MMMM dd, yyyy"), 48, 116, subPaint);

            // Divider line
            using var linePaint = new SKPaint { Color = SKColor.Parse("#1e293b"), StrokeWidth = 2 };
            canvas.DrawLine(48, 136, width - 48, 136, linePaint);

            // Footer tagline
            using var footerPaint = new SKPaint
            {
                Color    = SKColor.Parse("#334155"),
                TextSize = 22,
                IsAntialias = true
            };
            canvas.DrawText("\"Consistency beats intensity — show up every day, even at 80%\"", 48, height - 36, footerPaint);

            // Encode and share
            using var image  = surface.Snapshot();
            using var data   = image.Encode(SKEncodedImageFormat.Png, 100);
            var filePath = Path.Combine(FileSystem.CacheDirectory, $"weekly_summary_{DateTime.Now:yyyyMMdd_HHmm}.png");

            await using var stream = File.Create(filePath);
            data.SaveTo(stream);

            // Share via MAUI Share API
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "My Weekly Blueprint Summary",
                File  = new ShareFile(filePath)
            });

            return filePath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ExportService error: {ex.Message}");
            await Shell.Current.DisplayAlert("Export Error",
                "Could not generate the summary image. Please try again.", "OK");
            return null;
        }
    }
}
