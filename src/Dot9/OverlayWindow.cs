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
    private bool _fullscreenHintShown;

    /// <summary>Raised once per overlay session when an exclusive-fullscreen app is detected (the overlay cannot draw over it).</summary>
    public event EventHandler? ExclusiveFullscreenDetected;

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
            _fullscreenHintShown = false;
            FitToVirtualScreen();
            Show();
            ApplyClickThroughStyles();
            StartCompatibilityWatch();
            MaintainOverlayPlacement();
            StartFastCompatibilityRetry();
            CheckForExclusiveFullscreen();
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

    /// <summary>Initial WPF (DIP) sizing used before the window handle exists; covers the whole virtual screen.</summary>
    private void FitToVirtualScreen()
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

        ApplyClickThroughStyles();
        ShowWindow(hwnd, SwShowNoActivate);

        // Assert the window bounds in physical pixels rather than WPF DIPs:
        // SystemParameters.VirtualScreen* is expressed in the primary monitor's DPI,
        // which under-covers secondary monitors running a different scale factor.
        var x = GetSystemMetrics(SmXVirtualScreen);
        var y = GetSystemMetrics(SmYVirtualScreen);
        var cx = GetSystemMetrics(SmCxVirtualScreen);
        var cy = GetSystemMetrics(SmCyVirtualScreen);
        SetWindowPos(hwnd, HwndTopmost, x, y, cx, cy, SwpNoActivate | SwpShowWindow);
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

        if (_foregroundHook == IntPtr.Zero)
        {
            Dot9.Services.Log.Warn("Foreground WinEvent hook could not be installed; overlay may not reassert itself over some apps.");
        }
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
        CheckForExclusiveFullscreen();
    }

    /// <summary>
    /// Asks Windows whether a Direct3D exclusive-fullscreen app is running. Borderless/windowed
    /// fullscreen does NOT report this, so this only fires for the case where the overlay genuinely
    /// cannot appear — at which point we raise a one-time hint to switch to borderless.
    /// </summary>
    private void CheckForExclusiveFullscreen()
    {
        if (_fullscreenHintShown || !IsVisible)
        {
            return;
        }

        try
        {
            if (SHQueryUserNotificationState(out var state) == 0 &&
                state == QueryUserNotificationState.RunningD3DFullScreen)
            {
                _fullscreenHintShown = true;
                Dot9.Services.Log.Info("Exclusive fullscreen app detected; overlay cannot draw over it.");
                ExclusiveFullscreenDetected?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            Dot9.Services.Log.Warn("Could not query fullscreen notification state.", ex);
        }
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
