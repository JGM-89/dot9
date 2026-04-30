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
        var frameBrush = new SolidColorBrush(MediaColor.FromRgb(18, 23, 28));
        var screenBrush = new LinearGradientBrush(MediaColor.FromRgb(28, 35, 42), MediaColor.FromRgb(12, 15, 18), 90);
        var borderPen = new System.Windows.Media.Pen(new SolidColorBrush(MediaColor.FromRgb(71, 84, 96)), 1);

        dc.DrawRoundedRectangle(frameBrush, null, screen, 14, 14);
        screen.Inflate(-8, -8);
        dc.DrawRoundedRectangle(screenBrush, borderPen, screen, 9, 9);

        if (Settings is not null)
        {
            var scale = Math.Min(screen.Width / SystemParameters.PrimaryScreenWidth, screen.Height / SystemParameters.PrimaryScreenHeight);
            DotOverlayRenderer.Draw(dc, screen, Settings, Math.Max(0.18, scale));
        }
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
