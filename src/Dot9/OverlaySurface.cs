using System.Windows;
using System.Windows.Media;
using Dot9.Models;
using Dot9.Rendering;

namespace Dot9;

public sealed class OverlaySurface : FrameworkElement
{
    public static readonly DependencyProperty SettingsProperty =
        DependencyProperty.Register(
            nameof(Settings),
            typeof(Dot9Settings),
            typeof(OverlaySurface),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public Dot9Settings? Settings
    {
        get => (Dot9Settings?)GetValue(SettingsProperty);
        set => SetValue(SettingsProperty, value);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        if (Settings is null)
        {
            return;
        }

        DotOverlayRenderer.Draw(drawingContext, new Rect(0, 0, ActualWidth, ActualHeight), Settings.Dots);
    }
}
