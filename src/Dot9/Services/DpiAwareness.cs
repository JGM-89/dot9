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
        catch
        {
            // Fall back below for older Windows builds.
        }

        try
        {
            SetProcessDpiAwareness(ProcessDpiAwareness.PerMonitorDpiAware);
        }
        catch
        {
            // DPI awareness is a compatibility improvement; failure should not prevent startup.
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
