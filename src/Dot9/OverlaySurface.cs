using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Dot9.Models;
using Dot9.Rendering;
using WpfPoint = System.Windows.Point;
using static Dot9.Interop.NativeMethods;

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

        var screens = System.Windows.Forms.Screen.AllScreens;
        var targets = screens.Where(ShouldDrawOnScreen).ToList();
        if (targets.Count == 0)
        {
            // The saved display is gone (re-dock, driver update); don't silently draw nothing.
            targets = screens.Where(s => s.Primary).ToList();
        }

        foreach (var screen in targets)
        {
            var rect = GetScreenRectInDips(screen);
            DotOverlayRenderer.Draw(drawingContext, rect, Settings);
        }
    }

    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);
        InvalidateVisual();
    }

    /// <summary>
    /// Converts a monitor's pixel bounds into this element's DIPs. The overlay is a single
    /// window, and WPF renders a window at one DPI (the window's), so every monitor must be
    /// converted with the window's transform — not each monitor's own DPI.
    /// </summary>
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
}
