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

        var toggleBinding   = _state.Settings.Hotkeys.ToggleOverlay;
        var emergencyBinding = _state.Settings.Hotkeys.EmergencyOff;
        if (toggleBinding.Equals(emergencyBinding))
        {
            _state.SetHotkeyStatus("Choose different shortcuts for toggle and Emergency Off.", true);
            return;
        }

        var toggle    = ToWin32(toggleBinding);
        var emergency = ToWin32(emergencyBinding);

        var toggleRegistered    = RegisterHotKey(_handle, ToggleHotkeyId,    toggle.Modifiers    | ModNoRepeat, toggle.VirtualKey);
        var emergencyRegistered = RegisterHotKey(_handle, EmergencyHotkeyId, emergency.Modifiers | ModNoRepeat, emergency.VirtualKey);

        if (toggleRegistered && emergencyRegistered)
        {
            _state.SetHotkeyStatus("Hotkeys ready");
            return;
        }

        var failed = new List<string>();
        if (!toggleRegistered)
        {
            failed.Add($"toggle ({toggleBinding.DisplayName})");
        }

        if (!emergencyRegistered)
        {
            failed.Add($"Emergency Off ({emergencyBinding.DisplayName})");
        }

        _state.SetHotkeyStatus($"Could not register {string.Join(" and ", failed)}. Pick another shortcut.", true);
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

    private static (uint Modifiers, uint VirtualKey) ToWin32(HotkeyBinding binding)
    {
        uint mods = 0;
        if ((binding.Modifiers & ModifierKeys.Alt)     != 0) mods |= ModAlt;
        if ((binding.Modifiers & ModifierKeys.Control) != 0) mods |= ModControl;
        return (mods, (uint)KeyInterop.VirtualKeyFromKey(binding.Key));
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
