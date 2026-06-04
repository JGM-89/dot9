using System.Runtime.InteropServices;

namespace Dot9.Services;

public static class DpiAwareness
{
    private static readonly IntPtr DpiAwarenessContextPerMonitorAwareV2 = new(-4);

    public static void EnablePerMonitorAwareness()
    {
        try
        {
            if (SetProcessDpiAwarenessContext(DpiAwarenessContextPerMonitorAwareV2))
            {
                return;
            }
        }
        catch (Exception ex)
        {
            // Fall back below for older Windows builds.
            Log.Warn("Per-monitor-v2 DPI awareness unavailable; falling back.", ex);
        }

        try
        {
            SetProcessDpiAwareness(ProcessDpiAwareness.PerMonitorDpiAware);
        }
        catch (Exception ex)
        {
            // DPI awareness is a compatibility improvement; failure should not prevent startup.
            Log.Warn("Could not enable per-monitor DPI awareness.", ex);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetProcessDpiAwarenessContext(IntPtr dpiContext);

    [DllImport("shcore.dll", SetLastError = true)]
    private static extern int SetProcessDpiAwareness(ProcessDpiAwareness awareness);

    private enum ProcessDpiAwareness
    {
        DpiUnaware = 0,
        SystemDpiAware = 1,
        PerMonitorDpiAware = 2
    }
}
