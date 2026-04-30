using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Win32;

namespace Dot9.Models;

public sealed class AppState : INotifyPropertyChanged
{
    private Dot9Settings _settings = Dot9Settings.CreateDefault();
    private bool _overlayEnabled;
    private string _hotkeyStatusText = "Hotkeys ready";
    private bool _hasHotkeyWarning;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? SettingsChanged;
    public event EventHandler? OverlayEnabledChanged;
    public event EventHandler? HotkeysChanged;

    public Dot9Settings Settings
    {
        get => _settings;
        set
        {
            var oldToggle = _settings.Hotkeys.ToggleOverlay;
            var oldEmergency = _settings.Hotkeys.EmergencyOff;
            _settings = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ActivePresetName));
            OnPropertyChanged(nameof(ActiveModeName));
            SettingsChanged?.Invoke(this, EventArgs.Empty);
            if (oldToggle != _settings.Hotkeys.ToggleOverlay || oldEmergency != _settings.Hotkeys.EmergencyOff)
            {
                HotkeysChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public bool OverlayEnabled
    {
        get => _overlayEnabled;
        private set
        {
            if (_overlayEnabled == value)
            {
                return;
            }

            _overlayEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(OverlayStatusText));
            OverlayEnabledChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string OverlayStatusText => OverlayEnabled ? "On" : "Off";
    public string ActivePresetName => Settings.ActivePreset;
    public string ActiveModeName => Settings.MotionMode.GetDisplayName();
    public string HotkeyStatusText => _hotkeyStatusText;
    public bool HasHotkeyWarning => _hasHotkeyWarning;

    public void ToggleOverlay() => SetOverlayEnabled(!OverlayEnabled);

    public void SetOverlayEnabled(bool enabled) => OverlayEnabled = enabled;

    public void EmergencyOff() => OverlayEnabled = false;

    public void Update(Action<Dot9Settings> update)
    {
        var oldToggle = Settings.Hotkeys.ToggleOverlay;
        var oldEmergency = Settings.Hotkeys.EmergencyOff;
        update(Settings);
        OnPropertyChanged(nameof(Settings));
        OnPropertyChanged(nameof(ActivePresetName));
        OnPropertyChanged(nameof(ActiveModeName));
        SettingsChanged?.Invoke(this, EventArgs.Empty);
        if (oldToggle != Settings.Hotkeys.ToggleOverlay || oldEmergency != Settings.Hotkeys.EmergencyOff)
        {
            HotkeysChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ApplyPreset(PresetDefinition preset)
    {
        Settings = preset.CreateSettings();
    }

    public void SetHotkeyStatus(string message, bool isWarning = false)
    {
        _hotkeyStatusText = message;
        _hasHotkeyWarning = isWarning;
        OnPropertyChanged(nameof(HotkeyStatusText));
        OnPropertyChanged(nameof(HasHotkeyWarning));
    }

    public void ApplyReducedMotionPreference()
    {
        try
        {
            var value = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Accessibility", "AnimationEffects", null);
            if (value is int animationEffects && animationEffects == 0)
            {
                Settings.AllAnimationsEnabled = false;
            }
        }
        catch
        {
            Settings.AllAnimationsEnabled = false;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
