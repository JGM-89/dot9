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
    private const int MonitorDefaultToNearest = 2;

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
        if (TryGetPerMonitorRectInDips(screen, out var perMonitorRect))
        {
            return perMonitorRect;
        }

        var transform = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformFromDevice ?? Matrix.Identity;
        var virtualLeft = GetSystemMetrics(SmXVirtualScreen);
        var virtualTop = GetSystemMetrics(SmYVirtualScreen);
        var topLeft = transform.Transform(new WpfPoint(screen.Bounds.Left - virtualLeft, screen.Bounds.Top - virtualTop));
        var bottomRight = transform.Transform(new WpfPoint(screen.Bounds.Right - virtualLeft, screen.Bounds.Bottom - virtualTop));
        return new Rect(topLeft, bottomRight);
    }

    private bool TryGetPerMonitorRectInDips(System.Windows.Forms.Screen screen, out Rect rect)
    {
        rect = Rect.Empty;
        try
        {
            var monitor = MonitorFromPoint(new NativePoint(screen.Bounds.Left, screen.Bounds.Top), MonitorDefaultToNearest);
            if (monitor == IntPtr.Zero || GetDpiForMonitor(monitor, MonitorDpiType.EffectiveDpi, out var dpiX, out var dpiY) != 0 || dpiX == 0 || dpiY == 0)
            {
                return false;
            }

            var virtualLeft = GetSystemMetrics(SmXVirtualScreen);
            var virtualTop = GetSystemMetrics(SmYVirtualScreen);
            var topLeft = new WpfPoint(
                (screen.Bounds.Left - virtualLeft) * 96.0 / dpiX,
                (screen.Bounds.Top - virtualTop) * 96.0 / dpiY);
            var bottomRight = new WpfPoint(
                (screen.Bounds.Right - virtualLeft) * 96.0 / dpiX,
                (screen.Bounds.Bottom - virtualTop) * 96.0 / dpiY);
            rect = new Rect(topLeft, bottomRight);
            return true;
        }
        catch
        {
            return false;
        }
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

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(NativePoint pt, int flags);

    [DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr hmonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public NativePoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X;
        public int Y;
    }

    private enum MonitorDpiType
    {
        EffectiveDpi = 0
    }
}
