using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Dot9.Models;

namespace Dot9.Services;

public sealed class HotkeyService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModNoRepeat = 0x4000;
    private const int ToggleHotkeyId = 9001;
    private const int EmergencyHotkeyId = 9002;
    private readonly Window _owner;
    private readonly AppState _state;
    private HwndSource? _source;
    private IntPtr _handle;
    private bool _isHooked;

    public event EventHandler? ToggleRequested;
    public event EventHandler? EmergencyOffRequested;

    public HotkeyService(Window owner, AppState state)
    {
        _owner = owner;
        _state = state;
        _state.SettingsChanged += (_, _) => Register();
    }

    public void Register()
    {
        _handle = new WindowInteropHelper(_owner).EnsureHandle();
        _source = HwndSource.FromHwnd(_handle);
        if (_source is not null && !_isHooked)
        {
            _source.AddHook(HandleMessage);
            _isHooked = true;
        }

        UnregisterHotKey(_handle, ToggleHotkeyId);
        UnregisterHotKey(_handle, EmergencyHotkeyId);

        var toggle = Resolve(_state.Settings.Hotkeys.ToggleOverlay);
        var emergency = Resolve(_state.Settings.Hotkeys.EmergencyOff);
        RegisterHotKey(_handle, ToggleHotkeyId, toggle.Modifiers | ModNoRepeat, toggle.VirtualKey);
        RegisterHotKey(_handle, EmergencyHotkeyId, emergency.Modifiers | ModNoRepeat, emergency.VirtualKey);
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
        _isHooked = false;
    }

    private static (uint Modifiers, uint VirtualKey) Resolve(HotkeyChoice choice)
    {
        var key = choice switch
        {
            HotkeyChoice.CtrlAltD => Key.D,
            HotkeyChoice.CtrlAltO => Key.O,
            HotkeyChoice.F8 => Key.F8,
            HotkeyChoice.F9 => Key.F9,
            HotkeyChoice.F10 => Key.F10,
            HotkeyChoice.F12 => Key.F12,
            HotkeyChoice.CtrlAltBackspace => Key.Back,
            _ => Key.F8
        };

        var modifiers = choice is HotkeyChoice.CtrlAltD or HotkeyChoice.CtrlAltO or HotkeyChoice.CtrlAltBackspace
            ? ModControl | ModAlt
            : 0;

        return (modifiers, (uint)KeyInterop.VirtualKeyFromKey(key));
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
