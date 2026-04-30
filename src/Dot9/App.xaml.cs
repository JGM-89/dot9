using System.Windows;
using Dot9.Models;
using Dot9.Services;

namespace Dot9;

public partial class App : System.Windows.Application
{
    private SettingsStore? _settingsStore;
    private OverlayWindow? _overlayWindow;
    private MainWindow? _mainWindow;
    private HotkeyService? _hotkeyService;
    private TrayService? _trayService;

    public AppState State { get; } = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _settingsStore = new SettingsStore();
        State.Settings = _settingsStore.Load();
        State.ApplyReducedMotionPreference();

        _overlayWindow = new OverlayWindow(State);
        _mainWindow = new MainWindow(State);

        State.SettingsChanged += (_, _) =>
        {
            _overlayWindow.RefreshOverlay();
            _settingsStore.Save(State.Settings);
        };

        State.OverlayEnabledChanged += (_, _) => _overlayWindow.SetOverlayVisible(State.OverlayEnabled);

        _hotkeyService = new HotkeyService(_mainWindow, State);
        _hotkeyService.ToggleRequested += (_, _) => State.ToggleOverlay();
        _hotkeyService.EmergencyOffRequested += (_, _) => State.EmergencyOff();
        _hotkeyService.Register();

        _trayService = new TrayService(State, ShowSettings, Quit);
        _mainWindow.StateChanged += (_, _) =>
        {
            if (_mainWindow.WindowState == WindowState.Minimized)
            {
                _trayService.ShowMinimizedToTrayNotice();
            }
        };

        if (State.Settings.StartOverlayEnabled)
        {
            State.SetOverlayEnabled(true);
        }

        _mainWindow.Show();
    }

    private void ShowSettings()
    {
        if (_mainWindow is null)
        {
            return;
        }

        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void Quit()
    {
        _hotkeyService?.Dispose();
        _trayService?.Dispose();
        _settingsStore?.Save(State.Settings);
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        State.EmergencyOff();
        _hotkeyService?.Dispose();
        _trayService?.Dispose();
        _settingsStore?.Save(State.Settings);
        base.OnExit(e);
    }
}
