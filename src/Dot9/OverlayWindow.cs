using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Dot9.Models;
using static Dot9.Interop.NativeMethods;

namespace Dot9;

public sealed class OverlayWindow : Window
{
    private readonly AppState _state;
    private readonly OverlaySurface _surface;
    private readonly DispatcherTimer _topmostTimer;
    private readonly WinEventDelegate _foregroundChanged;
    private IntPtr _foregroundHook;
    private int _fastRetryTicksRemaining;

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
        _topmostTimer.Tick += (_, _) => MaintainOverlayPlacementRetry();
        _foregroundChanged = (_, _, _, _, _, _, _) => Dispatcher.InvokeAsync(StartFastCompatibilityRetry);
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
            StartFastCompatibilityRetry();
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
        _fastRetryTicksRemaining = 0;
        if (_foregroundHook == IntPtr.Zero)
        {
            return;
        }

        UnhookWinEvent(_foregroundHook);
        _foregroundHook = IntPtr.Zero;
    }

    private void StartFastCompatibilityRetry()
    {
        if (!IsVisible)
        {
            return;
        }

        _fastRetryTicksRemaining = 12;
        _topmostTimer.Interval = TimeSpan.FromMilliseconds(350);
        _topmostTimer.Start();
        MaintainOverlayPlacement();
    }

    private void MaintainOverlayPlacementRetry()
    {
        MaintainOverlayPlacement();
        _fastRetryTicksRemaining--;
        if (_fastRetryTicksRemaining > 0)
        {
            return;
        }

        _topmostTimer.Stop();
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
}
