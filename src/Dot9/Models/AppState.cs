using System.ComponentModel;
using System.Reflection;
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
    private bool _showOnboarding;
    private int _onboardingStep;

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
            OnPropertyChanged(string.Empty); // wholesale change (preset/load): refresh every bound property
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
            OnPropertyChanged(nameof(OverlayStatusLine));
            OnPropertyChanged(nameof(ToggleButtonText));
            OverlayEnabledChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string OverlayStatusText => OverlayEnabled ? "On" : "Off";

    /// <summary>App version for display, resolved from the assembly (single source of truth: the .csproj Version).</summary>
    public string AppVersion { get; } = ResolveAppVersion();

    public string ActivePresetName => Settings.ActivePreset;
    public string ActiveModeName => Settings.MotionMode.GetDisplayName();
    public string HotkeyStatusText => _hotkeyStatusText;
    public bool HasHotkeyWarning => _hasHotkeyWarning;

    // ── TopBar (derived view state) ──────────────────────
    public string OverlayStatusLine => OverlayEnabled
        ? $"Overlay on · {ActivePresetName}"
        : $"Overlay off · {ActivePresetName}";

    public string ToggleButtonText => OverlayEnabled ? "Turn overlay off" : "Turn overlay on";

    public string HotkeySummary =>
        $"{Settings.Hotkeys.ToggleOverlay.DisplayName} toggle  ·  {Settings.Hotkeys.EmergencyOff.DisplayName} off";

    // ── Two-way bound settings (route through Update so the
    //    save/overlay pipeline and change events still fire) ──
    public bool DotsEnabled       { get => Settings.Dots.Enabled;      set => Set(s => s.Dots.Enabled = value); }
    public double DotOpacity      { get => Settings.Dots.Opacity;      set => Set(s => s.Dots.Opacity = Math.Clamp(value, 0, 1)); }
    public double DotSize         { get => Settings.Dots.Size;         set => Set(s => s.Dots.Size = Math.Round(value, 1)); }
    public double DotEdgeDistance { get => Settings.Dots.EdgeDistance; set => Set(s => s.Dots.EdgeDistance = Math.Round(value, 1)); }
    public double DotsPerEdge     { get => Settings.Dots.DotsPerEdge;  set => Set(s => s.Dots.DotsPerEdge = (int)Math.Round(value)); }
    public EdgeSelection DotEdges { get => Settings.Dots.Edges;        set => Set(s => s.Dots.Edges = value); }
    public DotShape DotShape      { get => Settings.Dots.Shape;        set => Set(s => s.Dots.Shape = value); }
    public string DotColor
    {
        get => Settings.Dots.Color;
        set { if (value is not null) Set(s => s.Dots.Color = value); }
    }

    public bool CentreEnabled              { get => Settings.CentreAnchor.Enabled; set => Set(s => s.CentreAnchor.Enabled = value); }
    public double CentreOpacity            { get => Settings.CentreAnchor.Opacity; set => Set(s => s.CentreAnchor.Opacity = Math.Clamp(value, 0, 1)); }
    public double CentreSize               { get => Settings.CentreAnchor.Size;    set => Set(s => s.CentreAnchor.Size = Math.Round(value, 1)); }
    public CentreAnchorShape CentreShape   { get => Settings.CentreAnchor.Shape;   set => Set(s => s.CentreAnchor.Shape = value); }

    public bool HorizonEnabled       { get => Settings.Horizon.Enabled;          set => Set(s => s.Horizon.Enabled = value); }
    public double HorizonOpacity     { get => Settings.Horizon.Opacity;          set => Set(s => s.Horizon.Opacity = Math.Clamp(value, 0, 1)); }
    public double HorizonPosition    { get => Settings.Horizon.VerticalPosition; set => Set(s => s.Horizon.VerticalPosition = Math.Round(value, 1)); }
    public double HorizonWidth       { get => Settings.Horizon.Width;            set => Set(s => s.Horizon.Width = Math.Round(value, 1)); }
    public HorizonStyle HorizonStyle { get => Settings.Horizon.Style;            set => Set(s => s.Horizon.Style = value); }

    public bool VignetteEnabled    { get => Settings.Vignette.Enabled; set => Set(s => s.Vignette.Enabled = value); }
    public double VignetteOpacity  { get => Settings.Vignette.Opacity; set => Set(s => s.Vignette.Opacity = Math.Clamp(value, 0, 1)); }
    public double VignetteRadius   { get => Settings.Vignette.Radius;  set => Set(s => s.Vignette.Radius = Math.Round(value, 1)); }

    public string MonitorId
    {
        get => Settings.MonitorId;
        set { if (value is not null) Set(s => s.MonitorId = value); }
    }

    public bool AutoUpdateEnabled
    {
        get => Settings.AutoUpdate;
        set => Set(s => s.AutoUpdate = value);
    }

    private string _updateStatusText = "";
    public string UpdateStatusText
    {
        get => _updateStatusText;
        set { _updateStatusText = value; OnPropertyChanged(); }
    }

    private void Set(Action<Dot9Settings> mutate, [CallerMemberName] string? name = null)
    {
        Update(mutate);
        OnPropertyChanged(name);
    }

    public bool ShowOnboarding
    {
        get => _showOnboarding;
        set
        {
            _showOnboarding = value;
            OnPropertyChanged();
        }
    }

    public int OnboardingStep
    {
        get => _onboardingStep;
        set
        {
            _onboardingStep = value;
            OnPropertyChanged();
        }
    }

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
        var s = preset.CreateSettings();
        s.HasSeenOnboarding = Settings.HasSeenOnboarding;
        s.MonitorId = Settings.MonitorId;
        Settings = s;
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

    private static string ResolveAppVersion()
    {
        var assembly = typeof(AppState).Assembly;
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informational))
        {
            var plus = informational.IndexOf('+'); // strip build metadata (e.g. "+<commit>")
            return plus >= 0 ? informational[..plus] : informational;
        }
        return assembly.GetName().Version?.ToString(3) ?? "1.0.0";
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
