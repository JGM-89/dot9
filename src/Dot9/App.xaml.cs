using System.Windows;
using System.Windows.Threading;
using Dot9.Models;
using Dot9.Services;
using Dot9.Windows;
using WpfButton = System.Windows.Controls.Button;

namespace Dot9;

public partial class App : System.Windows.Application
{
    private SettingsStore? _settingsStore;
    private OverlayWindow? _overlayWindow;
    private MainWindow? _mainWindow;
    private HotkeyService? _hotkeyService;
    private TrayService? _trayService;
    private DispatcherTimer? _settingsSaveTimer;
    private bool _hasPendingSettingsSave;
    private OnboardingOverlay? _onboarding;
    private TrayPopover? _trayPopover;

    public AppState State { get; } = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _settingsStore = new SettingsStore();
        State.Settings = _settingsStore.Load();
        State.ApplyReducedMotionPreference();

        _overlayWindow = new OverlayWindow(State);
        _mainWindow    = new MainWindow(State);

        _settingsSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(450) };
        _settingsSaveTimer.Tick += (_, _) => FlushSettings();

        State.SettingsChanged += (_, _) =>
        {
            _overlayWindow.RefreshOverlay();
            ScheduleSettingsSave();
        };

        State.OverlayEnabledChanged += (_, _) => _overlayWindow.SetOverlayVisible(State.OverlayEnabled);

        State.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(AppState.ShowOnboarding) && State.ShowOnboarding)
                ShowOnboarding();
        };

        _hotkeyService = new HotkeyService(_mainWindow, State);
        _hotkeyService.ToggleRequested      += (_, _) => State.ToggleOverlay();
        _hotkeyService.EmergencyOffRequested += (_, _) => State.EmergencyOff();
        State.HotkeysChanged += (_, _) => _hotkeyService.Register();
        _hotkeyService.Register();

        _trayService = new TrayService(State, ShowSettings, Quit);
        _mainWindow.StateChanged += (_, _) =>
        {
            if (_mainWindow.WindowState == WindowState.Minimized)
                _trayService.ShowMinimizedToTrayNotice();
        };

        if (State.Settings.StartOverlayEnabled)
            State.SetOverlayEnabled(true);

        _mainWindow.Show();

        if (!State.Settings.HasSeenOnboarding)
            ShowOnboarding();
    }

    public void ShowTrayPopover(Window owner, WpfButton anchorButton)
    {
        if (_trayPopover is { IsVisible: true })
        {
            _trayPopover.Close();
            return;
        }

        _trayPopover = new TrayPopover(State, ShowSettings);
        _trayPopover.Owner = owner;

        // Position below the anchor button
        var pt = anchorButton.PointToScreen(new System.Windows.Point(anchorButton.ActualWidth, anchorButton.ActualHeight));
        _trayPopover.Left = pt.X - _trayPopover.Width;
        _trayPopover.Top  = pt.Y + 4;

        _trayPopover.Show();
        _trayPopover.Activate();
    }

    private void ShowOnboarding()
    {
        if (_mainWindow is null) return;
        if (_onboarding is { IsVisible: true }) return;

        _onboarding = new OnboardingOverlay(State)
        {
            Owner  = _mainWindow,
            Width  = _mainWindow.ActualWidth,
            Height = _mainWindow.ActualHeight,
            Left   = _mainWindow.Left,
            Top    = _mainWindow.Top
        };

        _mainWindow.LocationChanged += SyncOnboardingPosition;
        _mainWindow.SizeChanged     += SyncOnboardingSize;
        _onboarding.Closed += (_, _) =>
        {
            _mainWindow.LocationChanged -= SyncOnboardingPosition;
            _mainWindow.SizeChanged     -= SyncOnboardingSize;
        };

        _onboarding.Show();
    }

    private void SyncOnboardingPosition(object? sender, EventArgs e)
    {
        if (_onboarding is null || !_onboarding.IsVisible) return;
        _onboarding.Left = _mainWindow!.Left;
        _onboarding.Top  = _mainWindow.Top;
    }

    private void SyncOnboardingSize(object? sender, SizeChangedEventArgs e)
    {
        if (_onboarding is null || !_onboarding.IsVisible) return;
        _onboarding.Width  = _mainWindow!.ActualWidth;
        _onboarding.Height = _mainWindow.ActualHeight;
    }

    private void ShowSettings()
    {
        if (_mainWindow is null) return;
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void Quit()
    {
        _hotkeyService?.Dispose();
        _trayService?.Dispose();
        FlushSettings();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        State.EmergencyOff();
        _hotkeyService?.Dispose();
        _trayService?.Dispose();
        FlushSettings();
        base.OnExit(e);
    }

    private void ScheduleSettingsSave()
    {
        _hasPendingSettingsSave = true;
        _settingsSaveTimer?.Stop();
        _settingsSaveTimer?.Start();
    }

    private void FlushSettings()
    {
        _settingsSaveTimer?.Stop();
        if (!_hasPendingSettingsSave) return;
        _settingsStore?.Save(State.Settings);
        _hasPendingSettingsSave = false;
    }
}
