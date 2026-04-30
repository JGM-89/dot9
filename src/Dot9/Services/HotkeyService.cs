using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Dot9.Services;

public sealed class HotkeyService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const int ToggleHotkeyId = 9001;
    private const int EmergencyHotkeyId = 9002;
    private readonly Window _owner;
    private HwndSource? _source;
    private IntPtr _handle;

    public event EventHandler? ToggleRequested;
    public event EventHandler? EmergencyOffRequested;

    public HotkeyService(Window owner)
    {
        _owner = owner;
    }

    public void Register()
    {
        _handle = new WindowInteropHelper(_owner).EnsureHandle();
        _source = HwndSource.FromHwnd(_handle);
        _source?.AddHook(HandleMessage);

        RegisterHotKey(_handle, ToggleHotkeyId, ModControl | ModAlt, (uint)KeyInterop.VirtualKeyFromKey(System.Windows.Input.Key.D));
        RegisterHotKey(_handle, EmergencyHotkeyId, ModControl | ModAlt, (uint)KeyInterop.VirtualKeyFromKey(System.Windows.Input.Key.Back));
    }

    private IntPtr HandleMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg != WmHotkey)
        {
            return IntPtr.Zero;
        }

        var id = wParam.ToInt32();
        if (id == ToggleHotkeyId)
        {
            ToggleRequested?.Invoke(this, EventArgs.Empty);
            handled = true;
        }
        else if (id == EmergencyHotkeyId)
        {
            EmergencyOffRequested?.Invoke(this, EventArgs.Empty);
            handled = true;
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_handle != IntPtr.Zero)
        {
            UnregisterHotKey(_handle, ToggleHotkeyId);
            UnregisterHotKey(_handle, EmergencyHotkeyId);
        }

        _source?.RemoveHook(HandleMessage);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
