using System.Windows;
using System.Windows.Threading;
using Dot9.Models;
using Dot9.Services;
using Dot9.Windows;
using Velopack;
using WpfButton = System.Windows.Controls.Button;

namespace Dot9;

public partial class App : System.Windows.Application
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Velopack hooks (install / update / uninstall) must run first and may exit early.
        VelopackApp.Build().Run();

        var app = new App();
        app.InitializeComponent();
        app.Run();
    }

    private SettingsStore? _settingsStore;
    private OverlayWindow? _overlayWindow;
    private MainWindow? _mainWindow;
    private HotkeyService? _hotkeyService;
    private TrayService? _trayService;
    private DispatcherTimer? _settingsSaveTimer;
    private bool _hasPendingSettingsSave;
    private OnboardingOverlay? _onboarding;
    private TrayPopover? _trayPopover;
    private UpdateService? _updateService;

    public AppState State { get; } = new();

    static App()
    {
        DpiAwareness.EnablePerMonitorAwareness();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Log.Info($"Dot9 {State.AppVersion} starting.");

        // Crashes must leave a trace in the log — the app has no telemetry, so the
        // local log is the only way a user-reported crash can ever be diagnosed.
        DispatcherUnhandledException += (_, args) =>
            Log.Error("Unhandled UI exception; the app will exit.", args.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Log.Error("Unhandled exception; the app will exit.", args.ExceptionObject as Exception);

        _settingsStore = new SettingsStore();
        State.Settings = _settingsStore.Load();

        _overlayWindow = new OverlayWindow(State);
        _mainWindow    = new MainWindow(State);

        _settingsSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(450) };
        _settingsSaveTimer.Tick += (_, _) => FlushSettings();

        State.SettingsChanged += (_, _) =>
        {
            _overlayWindow.RefreshOverlay();
            ScheduleSettingsSave();
        };

        State.SettingsReplacing += (_, outgoing) => _settingsStore?.SaveBackup(outgoing);

        State.OverlayEnabledChanged += (_, _) =>
        {
            _overlayWindow.SetOverlayVisible(State.OverlayEnabled);
            // The Emergency Off key is only held while the overlay is on, so the
            // registrations must follow the overlay state.
            _hotkeyService?.Register();
        };

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
        _overlayWindow.ExclusiveFullscreenDetected += (_, _) => _trayService.ShowOverlayCoveredHint();
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

        SetupUpdates();
    }

    private void SetupUpdates()
    {
        _updateService = new UpdateService();
        _updateService.StatusChanged += (_, _) =>
            Dispatcher.InvokeAsync(() =>
            {
                State.UpdateStatusText = DescribeUpdateStatus(_updateService);
                State.UpdateReadyToApply = _updateService.Status == UpdateStatus.ReadyOnRestart;
            });
        _updateService.UpdateReady += (_, version) =>
            Dispatcher.InvokeAsync(() => _trayService?.ShowUpdateReadyHint(version));

        if (State.Settings.AutoUpdate)
        {
            _ = _updateService.CheckAndStageAsync();
        }
    }

    /// <summary>Manual "Check for updates" trigger from the About screen.</summary>
    public void CheckForUpdates()
    {
        if (_updateService is not null)
        {
            _ = _updateService.CheckAndStageAsync();
        }
    }

    /// <summary>Apply a staged update immediately and restart (the About "Restart now" button).</summary>
    public void RestartToApplyUpdate() => _updateService?.RestartToApply();

    private static string DescribeUpdateStatus(UpdateService service) => service.Status switch
    {
        UpdateStatus.Checking       => "Checking for updates…",
        UpdateStatus.Downloading    => "Downloading update…",
        UpdateStatus.ReadyOnRestart => $"Update {service.AvailableVersion} ready — restart Dot[9] to apply.",
        UpdateStatus.UpToDate       => "You're on the latest version.",
        UpdateStatus.Failed         => "Update check failed — see the log.",
        _ => ""
    };

    public void ShowTrayPopover(Window owner, WpfButton anchorButton)
    {
        if (_trayPopover is { IsVisible: true })
        {
            _trayPopover.Close();
            return;
        }

        _trayPopover = new TrayPopover(State, ShowSettings);
        _trayPopover.Owner = owner;

        // Position below the anchor button. PointToScreen returns physical pixels,
        // but Window.Left/Top are DIPs — convert, or the popover drifts on scaled displays.
        var devicePt = anchorButton.PointToScreen(new System.Windows.Point(anchorButton.ActualWidth, anchorButton.ActualHeight));
        var toDips = PresentationSource.FromVisual(anchorButton)?.CompositionTarget?.TransformFromDevice
                     ?? System.Windows.Media.Matrix.Identity;
        var pt = toDips.Transform(devicePt);
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
        _onboarding.Completed += () => _mainWindow?.NavigateToPresets();
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

    /// <summary>Called when the settings window hides to the tray (close or minimize) so the one-time balloon can explain where it went.</summary>
    public void NotifyHiddenToTray() => _trayService?.ShowMinimizedToTrayNotice();

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
