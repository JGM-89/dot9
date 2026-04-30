using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Dot9.Models;
using Dot9.Rendering;
using WpfPoint = System.Windows.Point;

namespace Dot9;

public sealed class OverlaySurface : FrameworkElement
{
    private const int SmXVirtualScreen = 76;
    private const int SmYVirtualScreen = 77;

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
            var rect = GetScreenRectInDips(screen);
            DotOverlayRenderer.Draw(drawingContext, rect, Settings);
        }
    }

    private Rect GetScreenRectInDips(System.Windows.Forms.Screen screen)
    {
        var transform = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformFromDevice ?? Matrix.Identity;
        var virtualLeft = GetSystemMetrics(SmXVirtualScreen);
        var virtualTop = GetSystemMetrics(SmYVirtualScreen);
        var topLeft = transform.Transform(new WpfPoint(screen.Bounds.Left - virtualLeft, screen.Bounds.Top - virtualTop));
        var bottomRight = transform.Transform(new WpfPoint(screen.Bounds.Right - virtualLeft, screen.Bounds.Bottom - virtualTop));
        return new Rect(topLeft, bottomRight);
    }

    private bool ShouldDrawOnScreen(System.Windows.Forms.Screen screen)
    {
        var monitorId = Settings?.MonitorId ?? "All";
        return monitorId == "All" ||
               screen.DeviceName.Equals(monitorId, StringComparison.OrdinalIgnoreCase) ||
               (monitorId == "Primary" && screen.Primary);
    }

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
}
