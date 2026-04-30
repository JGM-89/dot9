using System.Windows;
using System.Windows.Media;
using Dot9.Models;
using Dot9.Rendering;

namespace Dot9;

public sealed class PreviewSurface : FrameworkElement
{
    public static readonly DependencyProperty SettingsProperty =
        DependencyProperty.Register(
            nameof(Settings),
            typeof(Dot9Settings),
            typeof(PreviewSurface),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public Dot9Settings? Settings
    {
        get => (Dot9Settings?)GetValue(SettingsProperty);
        set => SetValue(SettingsProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var outer = new Rect(0, 0, ActualWidth, ActualHeight);
        var screen = FitRect(outer, 16.0 / 9.0);
        var frameBrush = new SolidColorBrush(Color.FromRgb(18, 23, 28));
        var screenBrush = new LinearGradientBrush(Color.FromRgb(28, 35, 42), Color.FromRgb(12, 15, 18), 90);
        var borderPen = new Pen(new SolidColorBrush(Color.FromRgb(71, 84, 96)), 1);

        dc.DrawRoundedRectangle(frameBrush, null, screen, 14, 14);
        screen.Inflate(-8, -8);
        dc.DrawRoundedRectangle(screenBrush, borderPen, screen, 9, 9);

        if (Settings is not null)
        {
            var scale = Math.Min(screen.Width / SystemParameters.PrimaryScreenWidth, screen.Height / SystemParameters.PrimaryScreenHeight);
            DotOverlayRenderer.Draw(dc, screen, Settings.Dots, Math.Max(0.18, scale));
        }

        DrawPreviewGameLines(dc, screen);
    }

    private static Rect FitRect(Rect bounds, double aspectRatio)
    {
        var width = bounds.Width;
        var height = width / aspectRatio;
        if (height > bounds.Height)
        {
            height = bounds.Height;
            width = height * aspectRatio;
        }

        return new Rect(
            bounds.Left + (bounds.Width - width) / 2,
            bounds.Top + (bounds.Height - height) / 2,
            width,
            height);
    }

    private static void DrawPreviewGameLines(DrawingContext dc, Rect screen)
    {
        var muted = new SolidColorBrush(Color.FromArgb(70, 135, 216, 232));
        var soft = new Pen(muted, 1);
        var horizonY = screen.Top + screen.Height * 0.54;
        dc.DrawLine(soft, new Point(screen.Left + screen.Width * 0.24, horizonY), new Point(screen.Right - screen.Width * 0.24, horizonY));

        var center = new Point(screen.Left + screen.Width / 2, screen.Top + screen.Height / 2);
        dc.DrawEllipse(null, new Pen(new SolidColorBrush(Color.FromArgb(110, 244, 247, 250)), 1.2), center, 5, 5);
    }
}
