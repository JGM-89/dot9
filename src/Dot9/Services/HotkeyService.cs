using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Dot9.Services;

public sealed class HotkeyService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModNoRepeat = 0x4000;
    private const int ToggleHotkeyId = 9001;
    private const int EmergencyHotkeyId = 9002;
    private const int ToggleFallbackHotkeyId = 9003;
    private const int EmergencyFallbackHotkeyId = 9004;
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

        RegisterHotKey(_handle, ToggleHotkeyId, ModControl | ModAlt | ModNoRepeat, (uint)KeyInterop.VirtualKeyFromKey(System.Windows.Input.Key.D));
        RegisterHotKey(_handle, EmergencyHotkeyId, ModControl | ModAlt | ModNoRepeat, (uint)KeyInterop.VirtualKeyFromKey(System.Windows.Input.Key.Back));
        RegisterHotKey(_handle, ToggleFallbackHotkeyId, ModNoRepeat, (uint)KeyInterop.VirtualKeyFromKey(System.Windows.Input.Key.F8));
        RegisterHotKey(_handle, EmergencyFallbackHotkeyId, ModNoRepeat, (uint)KeyInterop.VirtualKeyFromKey(System.Windows.Input.Key.F9));
    }

    private IntPtr HandleMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg != WmHotkey)
        {
            return IntPtr.Zero;
        }

        var id = wParam.ToInt32();
        if (id is ToggleHotkeyId or ToggleFallbackHotkeyId)
        {
            ToggleRequested?.Invoke(this, EventArgs.Empty);
            handled = true;
        }
        else if (id is EmergencyHotkeyId or EmergencyFallbackHotkeyId)
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
            UnregisterHotKey(_handle, ToggleFallbackHotkeyId);
            UnregisterHotKey(_handle, EmergencyFallbackHotkeyId);
        }

        _source?.RemoveHook(HandleMessage);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
