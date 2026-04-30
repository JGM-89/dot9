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

        foreach (var screen in System.Windows.Forms.Screen.AllScreens.Where(ShouldDrawOnScreen))
        {
            var left = screen.Bounds.Left - SystemParameters.VirtualScreenLeft;
            var top = screen.Bounds.Top - SystemParameters.VirtualScreenTop;
            var rect = new Rect(left, top, screen.Bounds.Width, screen.Bounds.Height);
            DotOverlayRenderer.Draw(drawingContext, rect, Settings);
        }
    }

    private bool ShouldDrawOnScreen(System.Windows.Forms.Screen screen)
    {
        var monitorId = Settings?.MonitorId ?? "All";
        return monitorId == "All" ||
               screen.DeviceName.Equals(monitorId, StringComparison.OrdinalIgnoreCase) ||
               (monitorId == "Primary" && screen.Primary);
    }
}
