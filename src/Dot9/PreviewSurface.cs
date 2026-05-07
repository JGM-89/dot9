using System.Windows;
using System.Windows.Media;
using Dot9.Models;
using Dot9.Rendering;
using MediaColor = System.Windows.Media.Color;

namespace Dot9;

public sealed class PreviewSurface : FrameworkElement
{
    public static readonly DependencyProperty SettingsProperty =
        DependencyProperty.Register(
            nameof(Settings),
            typeof(Dot9Settings),
            typeof(PreviewSurface),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty BackdropProperty =
        DependencyProperty.Register(
            nameof(Backdrop),
            typeof(PreviewBackdrop),
            typeof(PreviewSurface),
            new FrameworkPropertyMetadata(PreviewBackdrop.Scene, FrameworkPropertyMetadataOptions.AffectsRender));

    public Dot9Settings? Settings
    {
        get => (Dot9Settings?)GetValue(SettingsProperty);
        set => SetValue(SettingsProperty, value);
    }

    public PreviewBackdrop Backdrop
    {
        get => (PreviewBackdrop)GetValue(BackdropProperty);
        set => SetValue(BackdropProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var outer = new Rect(0, 0, ActualWidth, ActualHeight);
        var screen = FitRect(outer, 16.0 / 9.0);
        var frameBrush = new SolidColorBrush(MediaColor.FromRgb(18, 22, 20));
        var borderPen = new System.Windows.Media.Pen(new SolidColorBrush(MediaColor.FromArgb(0x1F, 0xFF, 0xF0, 0xD2)), 1);

        dc.DrawRoundedRectangle(frameBrush, null, screen, 14, 14);
        screen.Inflate(-8, -8);

        DrawScreenBackground(dc, screen);
        dc.DrawRoundedRectangle(null, borderPen, screen, 9, 9);

        if (Settings is not null)
        {
            var scale = Math.Min(screen.Width / SystemParameters.PrimaryScreenWidth, screen.Height / SystemParameters.PrimaryScreenHeight);
            DotOverlayRenderer.Draw(dc, screen, Settings, Math.Max(0.18, scale));
        }
    }

    private void DrawScreenBackground(DrawingContext dc, Rect screen)
    {
        switch (Backdrop)
        {
            case PreviewBackdrop.Black:
                dc.DrawRoundedRectangle(System.Windows.Media.Brushes.Black, null, screen, 9, 9);
                break;

            case PreviewBackdrop.Checker:
                dc.DrawRoundedRectangle(BuildCheckerBrush(), null, screen, 9, 9);
                break;

            case PreviewBackdrop.Scene:
            default:
                var sceneBrush = new LinearGradientBrush(
                    MediaColor.FromRgb(28, 35, 42),
                    MediaColor.FromRgb(12, 15, 18),
                    90);
                dc.DrawRoundedRectangle(sceneBrush, null, screen, 9, 9);
                break;
        }
    }

    private static DrawingBrush BuildCheckerBrush()
    {
        const double tile = 16;
        var light = MediaColor.FromRgb(0x11, 0x11, 0x11);
        var dark  = MediaColor.FromRgb(0x0A, 0x0A, 0x0A);

        var drawing = new DrawingGroup();
        using (var ctx = drawing.Open())
        {
            ctx.DrawRectangle(new SolidColorBrush(dark), null, new Rect(0, 0, tile * 2, tile * 2));
            ctx.DrawRectangle(new SolidColorBrush(light), null, new Rect(0, 0, tile, tile));
            ctx.DrawRectangle(new SolidColorBrush(light), null, new Rect(tile, tile, tile, tile));
        }

        return new DrawingBrush(drawing)
        {
            TileMode = TileMode.Tile,
            Viewport = new Rect(0, 0, tile * 2, tile * 2),
            ViewportUnits = BrushMappingMode.Absolute
        };
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
}
