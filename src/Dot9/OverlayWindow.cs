using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Dot9.Models;

namespace Dot9;

public sealed class OverlayWindow : Window
{
    private const int GwlExStyle = -20;
    private const int WsExTransparent = 0x00000020;
    private const int WsExLayered = 0x00080000;
    private const int WsExToolWindow = 0x00000080;
    private const int WsExNoActivate = 0x08000000;
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoMove = 0x0002;
    private const uint SwpNoActivate = 0x0010;
    private const uint SwpShowWindow = 0x0040;
    private const int SwShowNoActivate = 4;
    private const uint EventSystemForeground = 0x0003;
    private const uint WineventOutOfContext = 0x0000;
    private static readonly IntPtr HwndTopmost = new(-1);

    private readonly AppState _state;
    private readonly OverlaySurface _surface;
    private readonly DispatcherTimer _topmostTimer;
    private readonly WinEventDelegate _foregroundChanged;
    private IntPtr _foregroundHook;

    public OverlayWindow(AppState state)
    {
        _state = state;
        _surface = new OverlaySurface { Settings = state.Settings };

        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = System.Windows.Media.Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;
        ResizeMode = ResizeMode.NoResize;
        Focusable = false;
        IsHitTestVisible = false;
        Content = _surface;

        Loaded += (_, _) => ApplyClickThroughStyles();
        Closed += (_, _) => StopCompatibilityWatch();

        _topmostTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(350)
        };
        _topmostTimer.Tick += (_, _) => MaintainOverlayPlacement();
        _foregroundChanged = (_, _, _, _, _, _, _) => Dispatcher.InvokeAsync(MaintainOverlayPlacement);
    }

    public void SetOverlayVisible(bool visible)
    {
        if (visible)
        {
            FitToPrimaryScreen();
            Show();
            ApplyClickThroughStyles();
            StartCompatibilityWatch();
            MaintainOverlayPlacement();
        }
        else
        {
            StopCompatibilityWatch();
            Hide();
        }
    }

    public void RefreshOverlay()
    {
        _surface.Settings = null;
        _surface.Settings = _state.Settings;
    }

    private void FitToPrimaryScreen()
    {
        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;
    }

    private void MaintainOverlayPlacement()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero || !IsVisible)
        {
            return;
        }

        FitToPrimaryScreen();
        ApplyClickThroughStyles();
        ShowWindow(hwnd, SwShowNoActivate);
        SetWindowPos(hwnd, HwndTopmost, 0, 0, 0, 0, SwpNoMove | SwpNoSize | SwpNoActivate | SwpShowWindow);
    }

    private void StartCompatibilityWatch()
    {
        _topmostTimer.Start();
        if (_foregroundHook != IntPtr.Zero)
        {
            return;
        }

        _foregroundHook = SetWinEventHook(
            EventSystemForeground,
            EventSystemForeground,
            IntPtr.Zero,
            _foregroundChanged,
            0,
            0,
            WineventOutOfContext);
    }

    private void StopCompatibilityWatch()
    {
        _topmostTimer.Stop();
        if (_foregroundHook == IntPtr.Zero)
        {
            return;
        }

        UnhookWinEvent(_foregroundHook);
        _foregroundHook = IntPtr.Zero;
    }

    private void ApplyClickThroughStyles()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        var extendedStyle = GetWindowLong(hwnd, GwlExStyle);
        SetWindowLong(hwnd, GwlExStyle, extendedStyle | WsExTransparent | WsExLayered | WsExToolWindow | WsExNoActivate);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWinEventHook(
        uint eventMin,
        uint eventMax,
        IntPtr hmodWinEventProc,
        WinEventDelegate lpfnWinEventProc,
        uint idProcess,
        uint idThread,
        uint dwFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

    private delegate void WinEventDelegate(
        IntPtr hWinEventHook,
        uint eventType,
        IntPtr hwnd,
        int idObject,
        int idChild,
        uint dwEventThread,
        uint dwmsEventTime);
}
