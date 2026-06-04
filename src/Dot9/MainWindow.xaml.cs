using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WpfKey = System.Windows.Input.Key;
using WpfKeyboard = System.Windows.Input.Keyboard;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using Dot9.Models;
using Forms = System.Windows.Forms;
using WpfBrush = System.Windows.Media.Brush;
using WpfButton = System.Windows.Controls.Button;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfOrientation = System.Windows.Controls.Orientation;

namespace Dot9;

public partial class MainWindow : Window
{
    private readonly AppState _state;
    private bool _capturingToggle;
    private bool _capturingEmergency;

    private readonly Dictionary<string, string> _palettes = new()
    {
        ["amber"]  = "#E9B86E",
        ["warm"]   = "#EFEAE0",
        ["violet"] = "#B8A4FF",
        ["cyan"]   = "#87D8E8",
        ["green"]  = "#B7E4C7",
        ["grey"]   = "#8A8377"
    };

    public MainWindow(AppState state)
    {
        _state = state;
        InitializeComponent();

        MonitorCombo.ItemsSource = BuildMonitorChoices();
        DotSwatches.Palette = _palettes;

        DataContext = _state;

        BuildPresetCards();

        // Settings controls are bound directly to AppState; only the derived
        // bits that don't bind cleanly (live preview, hotkey button captions)
        // are refreshed here.
        _state.PropertyChanged += (_, _) => Dispatcher.InvokeAsync(RefreshDerivedUi);
        _state.SettingsChanged += (_, _) => Dispatcher.InvokeAsync(RefreshDerivedUi);
        RefreshDerivedUi();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        _state.EmergencyOff();
        base.OnClosing(e);
        System.Windows.Application.Current.Shutdown();
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        if (WindowState == WindowState.Minimized)
            Hide();
    }

    private void RefreshDerivedUi()
    {
        // The overlay settings are mutated in place, so the preview must be
        // told to repaint explicitly even when the Settings reference is unchanged.
        HomePreview.Settings = _state.Settings;
        HomePreview.InvalidateVisual();

        if (!_capturingToggle)
            ToggleHotkeyBtn.Content = _state.Settings.Hotkeys.ToggleOverlay.DisplayName;
        if (!_capturingEmergency)
            EmergencyHotkeyBtn.Content = _state.Settings.Hotkeys.EmergencyOff.DisplayName;
    }

    // ──────────────────────────────────────────────────
    // Navigation
    // ──────────────────────────────────────────────────

    private void ShowTune(object sender, RoutedEventArgs e)    => ShowView(TuneView, NavTune);
    private void ShowPresets(object sender, RoutedEventArgs e) => ShowView(PresetsView, NavPresets);
    private void ShowHotkeys(object sender, RoutedEventArgs e) => ShowView(HotkeysView, NavHotkeys);
    private void ShowSafety(object sender, RoutedEventArgs e)  => ShowView(SafetyView, NavSafety);
    private void ShowAbout(object sender, RoutedEventArgs e)   => ShowView(AboutView, NavAbout);

    private void ShowView(UIElement visible, WpfButton activeBtn)
    {
        TuneView.Visibility    = Visibility.Collapsed;
        PresetsView.Visibility = Visibility.Collapsed;
        HotkeysView.Visibility = Visibility.Collapsed;
        SafetyView.Visibility  = Visibility.Collapsed;
        AboutView.Visibility   = Visibility.Collapsed;
        visible.Visibility     = Visibility.Visible;

        var ghost  = (Style)FindResource("NavButton");
        var active = (Style)FindResource("NavButtonActive");

        NavTune.Style    = ghost;
        NavPresets.Style = ghost;
        NavHotkeys.Style = ghost;
        NavSafety.Style  = ghost;
        NavAbout.Style   = ghost;
        activeBtn.Style  = active;
    }

    public void NavigateToTune()    => ShowView(TuneView, NavTune);
    public void NavigateToPresets() => ShowView(PresetsView, NavPresets);
    public void NavigateToSafety()  => ShowView(SafetyView, NavSafety);

    private void ReplayWelcome(object sender, RoutedEventArgs e)
    {
        _state.ShowOnboarding  = true;
        _state.OnboardingStep  = 0;
    }

    private void ResetToDefaults_Click(object sender, RoutedEventArgs e)
    {
        _state.ApplyPreset(Presets.Gentle);
    }

    private void FooterSafetyNavigate(object sender, RequestNavigateEventArgs e)
    {
        ShowView(SafetyView, NavSafety);
        e.Handled = true;
    }

    private void OpenExternalLink(object sender, RequestNavigateEventArgs e)
    {
        e.Handled = true;
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            // A failed browser launch should not interrupt the settings UI.
            Dot9.Services.Log.Warn("Could not open external link.", ex);
        }
    }

    // ──────────────────────────────────────────────────
    // Overlay controls
    // ──────────────────────────────────────────────────

    private void ToggleOverlay(object sender, RoutedEventArgs e) => _state.ToggleOverlay();

    private void OpenTrayPopover(object sender, RoutedEventArgs e)
    {
        var app = (App)System.Windows.Application.Current;
        app.ShowTrayPopover(this, TrayPopoverBtn);
    }

    // ──────────────────────────────────────────────────
    // Backdrop (preview-only view state)
    // ──────────────────────────────────────────────────

    private void BackdropSceneChecked(object sender, RoutedEventArgs e)
    {
        if (HomePreview is not null) HomePreview.Backdrop = PreviewBackdrop.Scene;
    }

    private void BackdropBlackChecked(object sender, RoutedEventArgs e)
    {
        if (HomePreview is not null) HomePreview.Backdrop = PreviewBackdrop.Black;
    }

    private void BackdropCheckerChecked(object sender, RoutedEventArgs e)
    {
        if (HomePreview is not null) HomePreview.Backdrop = PreviewBackdrop.Checker;
    }

    // ──────────────────────────────────────────────────
    // Hotkey capture handlers
    // ──────────────────────────────────────────────────

    private void StartToggleHotkeyCapture(object sender, RoutedEventArgs e)
    {
        if (_capturingToggle || _capturingEmergency) return;
        _capturingToggle = true;
        ToggleHotkeyBtn.Content = "Press a key…";
        PreviewKeyDown += OnToggleHotkeyCaptureKeyDown;
    }

    private void StartEmergencyHotkeyCapture(object sender, RoutedEventArgs e)
    {
        if (_capturingToggle || _capturingEmergency) return;
        _capturingEmergency = true;
        EmergencyHotkeyBtn.Content = "Press a key…";
        PreviewKeyDown += OnEmergencyHotkeyCaptureKeyDown;
    }

    private void OnToggleHotkeyCaptureKeyDown(object sender, WpfKeyEventArgs e)
    {
        e.Handled = true;
        PreviewKeyDown -= OnToggleHotkeyCaptureKeyDown;
        _capturingToggle = false;

        var key = e.Key == WpfKey.System ? e.SystemKey : e.Key;
        if (key == WpfKey.Escape) { RefreshDerivedUi(); return; }
        if (IsModifierOnly(key)) { ToggleHotkeyBtn.Content = "Press a key…"; _capturingToggle = true; PreviewKeyDown += OnToggleHotkeyCaptureKeyDown; return; }

        var binding = new HotkeyBinding { Modifiers = WpfKeyboard.Modifiers, Key = key };
        if (binding.Equals(_state.Settings.Hotkeys.EmergencyOff))
        {
            _state.SetHotkeyStatus("Toggle and Emergency Off cannot share the same shortcut.", true);
            RefreshDerivedUi();
            return;
        }
        _state.Update(s => s.Hotkeys.ToggleOverlay = binding);
    }

    private void OnEmergencyHotkeyCaptureKeyDown(object sender, WpfKeyEventArgs e)
    {
        e.Handled = true;
        PreviewKeyDown -= OnEmergencyHotkeyCaptureKeyDown;
        _capturingEmergency = false;

        var key = e.Key == WpfKey.System ? e.SystemKey : e.Key;
        if (key == WpfKey.Escape) { RefreshDerivedUi(); return; }
        if (IsModifierOnly(key)) { EmergencyHotkeyBtn.Content = "Press a key…"; _capturingEmergency = true; PreviewKeyDown += OnEmergencyHotkeyCaptureKeyDown; return; }

        var binding = new HotkeyBinding { Modifiers = WpfKeyboard.Modifiers, Key = key };
        if (binding.Equals(_state.Settings.Hotkeys.ToggleOverlay))
        {
            _state.SetHotkeyStatus("Toggle and Emergency Off cannot share the same shortcut.", true);
            RefreshDerivedUi();
            return;
        }
        _state.Update(s => s.Hotkeys.EmergencyOff = binding);
    }

    private static bool IsModifierOnly(WpfKey key) =>
        key is WpfKey.LeftCtrl or WpfKey.RightCtrl
            or WpfKey.LeftAlt or WpfKey.RightAlt
            or WpfKey.LeftShift or WpfKey.RightShift
            or WpfKey.LWin or WpfKey.RWin;

    // ──────────────────────────────────────────────────
    // Presets
    // ──────────────────────────────────────────────────

    private void BuildPresetCards()
    {
        PresetGrid.Children.Clear();
        foreach (var preset in Presets.All)
        {
            var card = new Border
            {
                Style  = (Style)FindResource("PanelCard"),
                Margin = new Thickness(0, 0, 14, 14)
            };

            var stack = new StackPanel();

            // Header row: title + active badge
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var title = new TextBlock
            {
                Text       = preset.Name,
                FontSize   = 16,
                FontWeight = FontWeights.SemiBold
            };
            Grid.SetColumn(title, 0);
            headerGrid.Children.Add(title);

            var activeBadge = new TextBlock
            {
                Text         = "● ACTIVE",
                FontSize     = 11,
                FontFamily   = new WpfFontFamily("JetBrains Mono, Cascadia Code, Consolas"),
                Foreground   = (WpfBrush)FindResource("AccentBrush"),
                VerticalAlignment = VerticalAlignment.Center,
                Visibility   = _state.ActivePresetName == preset.ShortName ? Visibility.Visible : Visibility.Collapsed,
                Tag          = preset.ShortName
            };
            Grid.SetColumn(activeBadge, 1);
            headerGrid.Children.Add(activeBadge);
            stack.Children.Add(headerGrid);

            // Description
            stack.Children.Add(new TextBlock
            {
                Text         = preset.Description,
                Foreground   = (WpfBrush)FindResource("InkMutedBrush"),
                TextWrapping = TextWrapping.Wrap,
                FontSize     = 12.5,
                Margin       = new Thickness(0, 8, 0, 12),
                MinHeight    = 48
            });

            // Mini preview
            stack.Children.Add(new PreviewSurface
            {
                Settings = preset.CreateSettings(),
                Height   = 130,
                Margin   = new Thickness(0, 0, 0, 14)
            });

            // Buttons
            var buttons = new StackPanel { Orientation = WpfOrientation.Horizontal };

            var isActive = _state.ActivePresetName == preset.ShortName;
            var useBtn = new WpfButton
            {
                Content = "Use preset",
                Style   = isActive
                    ? (Style)FindResource(typeof(WpfButton))
                    : (Style)FindResource("PrimaryButton"),
                Margin  = new Thickness(0, 0, 10, 0)
            };
            useBtn.Click += (_, _) =>
            {
                _state.ApplyPreset(preset);
                BuildPresetCards();
            };

            var customBtn = new WpfButton { Content = "Customise →" };
            customBtn.Click += (_, _) =>
            {
                _state.ApplyPreset(preset);
                ShowView(TuneView, NavTune);
            };

            buttons.Children.Add(useBtn);
            buttons.Children.Add(customBtn);
            stack.Children.Add(buttons);

            card.Child = stack;
            PresetGrid.Children.Add(card);
        }
    }

    // ──────────────────────────────────────────────────
    // Monitor list
    // ──────────────────────────────────────────────────

    private static IReadOnlyList<MonitorChoice> BuildMonitorChoices()
    {
        var choices = new List<MonitorChoice>
        {
            new("All",     "All monitors"),
            new("Primary", "Primary monitor")
        };
        var index = 1;
        foreach (var screen in Forms.Screen.AllScreens)
        {
            choices.Add(new MonitorChoice(
                screen.DeviceName,
                $"Display {index}: {screen.Bounds.Width}×{screen.Bounds.Height}{(screen.Primary ? " (primary)" : "")}"));
            index++;
        }
        return choices;
    }

    private sealed record MonitorChoice(string Id, string Label)
    {
        public override string ToString() => Label;
    }
}
