using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WpfKey = System.Windows.Input.Key;
using WpfKeyboard = System.Windows.Input.Keyboard;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using WpfModifierKeys = System.Windows.Input.ModifierKeys;
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
    private bool _isRefreshing;
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
        _isRefreshing = true;
        InitializeComponent();
        DataContext = _state;

        MonitorCombo.ItemsSource = BuildMonitorChoices();

        DotSwatches.Palette = _palettes;

        BuildPresetCards();
        _state.PropertyChanged  += (_, _) => Dispatcher.InvokeAsync(RefreshUi);
        _state.SettingsChanged  += (_, _) => Dispatcher.InvokeAsync(RefreshUi);
        _isRefreshing = false;
        RefreshUi();
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

    // ──────────────────────────────────────────────────
    // RefreshUi
    // ──────────────────────────────────────────────────

    private void RefreshUi()
    {
        _isRefreshing = true;

        HomePreview.Settings = _state.Settings;
        HomePreview.InvalidateVisual();

        // TopBar
        var on = _state.OverlayEnabled;
        StatusDot.Fill = on
            ? (WpfBrush)FindResource("AccentBrush")
            : (WpfBrush)FindResource("InkDimBrush");

        OverlayStatusLabel.Text = on
            ? $"Overlay on · {_state.ActivePresetName}"
            : $"Overlay off · {_state.ActivePresetName}";

        HotkeySummaryLabel.Text =
            $"{_state.Settings.Hotkeys.ToggleOverlay.DisplayName} toggle  ·  " +
            $"{_state.Settings.Hotkeys.EmergencyOff.DisplayName} off";

        ToggleOverlayBtn.Content = on ? "Turn overlay off" : "Turn overlay on";
        ToggleOverlayBtn.Style   = on
            ? (Style)FindResource("PrimaryButton")
            : (Style)FindResource(typeof(WpfButton));

        // Dots
        DotsEnabledCheck.IsChecked   = _state.Settings.Dots.Enabled;
        OpacitySlider.Value          = Math.Round(_state.Settings.Dots.Opacity * 100);
        OpacityVal.Text              = $"{(int)Math.Round(_state.Settings.Dots.Opacity * 100)}%";
        SizeSlider.Value             = _state.Settings.Dots.Size;
        SizeVal.Text                 = $"{_state.Settings.Dots.Size:F0}px";
        DistanceSlider.Value         = _state.Settings.Dots.EdgeDistance;
        DistanceVal.Text             = $"{_state.Settings.Dots.EdgeDistance:F0}%";
        DotsPerEdgeSlider.Value      = _state.Settings.Dots.DotsPerEdge;
        DotsPerEdgeVal.Text          = $"{_state.Settings.Dots.DotsPerEdge}";

        SetEdgesRadio(_state.Settings.Dots.Edges);
        SetShapeRadio(_state.Settings.Dots.Shape);

        DotSwatches.SelectedColor = _state.Settings.Dots.Color;
        UpdateSectionBodyState(DotsBody, _state.Settings.Dots.Enabled);

        // Centre
        CentreAnchorCheck.IsChecked = _state.Settings.CentreAnchor.Enabled;
        CentreOpacitySlider.Value   = Math.Round(_state.Settings.CentreAnchor.Opacity * 100);
        CentreOpacityVal.Text       = $"{(int)Math.Round(_state.Settings.CentreAnchor.Opacity * 100)}%";
        CentreSizeSlider.Value      = _state.Settings.CentreAnchor.Size;
        CentreSizeVal.Text          = $"{_state.Settings.CentreAnchor.Size:F0}px";
        SetCentreShapeRadio(_state.Settings.CentreAnchor.Shape);
        UpdateSectionBodyState(CentreBody, _state.Settings.CentreAnchor.Enabled);

        // Horizon
        HorizonCheck.IsChecked       = _state.Settings.Horizon.Enabled;
        HorizonOpacitySlider.Value   = Math.Round(_state.Settings.Horizon.Opacity * 100);
        HorizonOpacityVal.Text       = $"{(int)Math.Round(_state.Settings.Horizon.Opacity * 100)}%";
        HorizonPositionSlider.Value  = _state.Settings.Horizon.VerticalPosition;
        HorizonPositionVal.Text      = $"{_state.Settings.Horizon.VerticalPosition:F0}%";
        HorizonWidthSlider.Value     = _state.Settings.Horizon.Width;
        HorizonWidthVal.Text         = $"{_state.Settings.Horizon.Width:F0}%";
        SetHorizonStyleRadio(_state.Settings.Horizon.Style);
        UpdateSectionBodyState(HorizonBody, _state.Settings.Horizon.Enabled);

        // Vignette
        VignetteCheck.IsChecked      = _state.Settings.Vignette.Enabled;
        VignetteOpacitySlider.Value  = Math.Round(_state.Settings.Vignette.Opacity * 100);
        VignetteOpacityVal.Text      = $"{(int)Math.Round(_state.Settings.Vignette.Opacity * 100)}%";
        VignetteRadiusSlider.Value   = _state.Settings.Vignette.Radius;
        VignetteRadiusVal.Text       = $"{_state.Settings.Vignette.Radius:F0}%";
        UpdateSectionBodyState(VignetteBody, _state.Settings.Vignette.Enabled);

        // Hotkeys
        if (!_capturingToggle)
            ToggleHotkeyBtn.Content   = _state.Settings.Hotkeys.ToggleOverlay.DisplayName;
        if (!_capturingEmergency)
            EmergencyHotkeyBtn.Content = _state.Settings.Hotkeys.EmergencyOff.DisplayName;
        if (_state.HasHotkeyWarning)
        {
            HotkeyWarningText.Text          = _state.HotkeyStatusText;
            HotkeyWarningBorder.Visibility  = Visibility.Visible;
        }
        else
        {
            HotkeyWarningBorder.Visibility = Visibility.Collapsed;
        }

        MonitorCombo.SelectedValue = _state.Settings.MonitorId;

        _isRefreshing = false;
    }

    private static void UpdateSectionBodyState(UIElement body, bool enabled)
    {
        body.Opacity  = enabled ? 1.0 : 0.45;
        body.IsEnabled = enabled;
    }

    // ──────────────────────────────────────────────────
    // Segmented radio helpers
    // ──────────────────────────────────────────────────

    private void SetEdgesRadio(EdgeSelection edges)
    {
        EdgesLR.IsChecked  = edges == EdgeSelection.LeftRight;
        EdgesTB.IsChecked  = edges == EdgeSelection.TopBottom;
        EdgesAll.IsChecked = edges == EdgeSelection.AllEdges;
    }

    private void SetShapeRadio(DotShape shape)
    {
        ShapeDot.IsChecked  = shape == DotShape.Circle;
        ShapeRing.IsChecked = shape == DotShape.Ring;
        ShapeSoft.IsChecked = shape == DotShape.SoftGlow;
    }

    private void SetCentreShapeRadio(CentreAnchorShape shape)
    {
        CentreRing.IsChecked  = shape == CentreAnchorShape.Ring;
        CentreDot.IsChecked   = shape == CentreAnchorShape.Dot;
        CentreCross.IsChecked = shape == CentreAnchorShape.Cross;
    }

    private void SetHorizonStyleRadio(HorizonStyle style)
    {
        HorizonFull.IsChecked  = style == HorizonStyle.FullLine;
        HorizonSplit.IsChecked = style == HorizonStyle.Segmented;
        HorizonTicks.IsChecked = style == HorizonStyle.SideTicks;
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
    // Backdrop
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
    // Edge dots event handlers
    // ──────────────────────────────────────────────────

    private void DotsEnabledChanged(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing) return;
        var enabled = DotsEnabledCheck.IsChecked == true;
        _state.Update(s => s.Dots.Enabled = enabled);
        UpdateSectionBodyState(DotsBody, enabled);
    }

    private void OpacityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        OpacityVal.Text = $"{(int)Math.Round(e.NewValue)}%";
        _state.Update(s => s.Dots.Opacity = Math.Clamp(e.NewValue / 100, 0, 1));
    }

    private void DotSizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        SizeVal.Text = $"{Math.Round(e.NewValue):F0}px";
        _state.Update(s => s.Dots.Size = Math.Round(e.NewValue, 1));
    }

    private void DistanceChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        DistanceVal.Text = $"{Math.Round(e.NewValue):F0}%";
        _state.Update(s => s.Dots.EdgeDistance = Math.Round(e.NewValue, 1));
    }

    private void DotsPerEdgeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        DotsPerEdgeVal.Text = $"{(int)Math.Round(e.NewValue)}";
        _state.Update(s => s.Dots.DotsPerEdge = (int)Math.Round(e.NewValue));
    }

    private void EdgesLRChecked(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing || EdgesLR.IsChecked != true) return;
        _state.Update(s => s.Dots.Edges = EdgeSelection.LeftRight);
    }

    private void EdgesTBChecked(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing || EdgesTB.IsChecked != true) return;
        _state.Update(s => s.Dots.Edges = EdgeSelection.TopBottom);
    }

    private void EdgesAllChecked(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing || EdgesAll.IsChecked != true) return;
        _state.Update(s => s.Dots.Edges = EdgeSelection.AllEdges);
    }

    private void ShapeDotChecked(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing || ShapeDot.IsChecked != true) return;
        _state.Update(s => s.Dots.Shape = DotShape.Circle);
    }

    private void ShapeRingChecked(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing || ShapeRing.IsChecked != true) return;
        _state.Update(s => s.Dots.Shape = DotShape.Ring);
    }

    private void ShapeSoftChecked(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing || ShapeSoft.IsChecked != true) return;
        _state.Update(s => s.Dots.Shape = DotShape.SoftGlow);
    }

    private void DotColorSwatchChanged(object sender, string? hex)
    {
        if (_isRefreshing || hex is null) return;
        _state.Update(s => s.Dots.Color = hex);
    }

    // ──────────────────────────────────────────────────
    // Centre anchor handlers
    // ──────────────────────────────────────────────────

    private void CentreAnchorChanged(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing) return;
        var enabled = CentreAnchorCheck.IsChecked == true;
        _state.Update(s => s.CentreAnchor.Enabled = enabled);
        UpdateSectionBodyState(CentreBody, enabled);
    }

    private void CentreOpacityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        CentreOpacityVal.Text = $"{(int)Math.Round(e.NewValue)}%";
        _state.Update(s => s.CentreAnchor.Opacity = Math.Clamp(e.NewValue / 100, 0, 1));
    }

    private void CentreSizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        CentreSizeVal.Text = $"{Math.Round(e.NewValue):F0}px";
        _state.Update(s => s.CentreAnchor.Size = Math.Round(e.NewValue, 1));
    }

    private void CentreShapeRingChecked(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing || CentreRing.IsChecked != true) return;
        _state.Update(s => s.CentreAnchor.Shape = CentreAnchorShape.Ring);
    }

    private void CentreShapeDotChecked(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing || CentreDot.IsChecked != true) return;
        _state.Update(s => s.CentreAnchor.Shape = CentreAnchorShape.Dot);
    }

    private void CentreShapeCrossChecked(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing || CentreCross.IsChecked != true) return;
        _state.Update(s => s.CentreAnchor.Shape = CentreAnchorShape.Cross);
    }

    // ──────────────────────────────────────────────────
    // Horizon handlers
    // ──────────────────────────────────────────────────

    private void HorizonChanged(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing) return;
        var enabled = HorizonCheck.IsChecked == true;
        _state.Update(s => s.Horizon.Enabled = enabled);
        UpdateSectionBodyState(HorizonBody, enabled);
    }

    private void HorizonOpacityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        HorizonOpacityVal.Text = $"{(int)Math.Round(e.NewValue)}%";
        _state.Update(s => s.Horizon.Opacity = Math.Clamp(e.NewValue / 100, 0, 1));
    }

    private void HorizonPositionChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        HorizonPositionVal.Text = $"{Math.Round(e.NewValue):F0}%";
        _state.Update(s => s.Horizon.VerticalPosition = Math.Round(e.NewValue, 1));
    }

    private void HorizonWidthChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        HorizonWidthVal.Text = $"{Math.Round(e.NewValue):F0}%";
        _state.Update(s => s.Horizon.Width = Math.Round(e.NewValue, 1));
    }

    private void HorizonStyleFullChecked(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing || HorizonFull.IsChecked != true) return;
        _state.Update(s => s.Horizon.Style = HorizonStyle.FullLine);
    }

    private void HorizonStyleSplitChecked(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing || HorizonSplit.IsChecked != true) return;
        _state.Update(s => s.Horizon.Style = HorizonStyle.Segmented);
    }

    private void HorizonStyleTicksChecked(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing || HorizonTicks.IsChecked != true) return;
        _state.Update(s => s.Horizon.Style = HorizonStyle.SideTicks);
    }

    // ──────────────────────────────────────────────────
    // Vignette handlers
    // ──────────────────────────────────────────────────

    private void VignetteChanged(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing) return;
        var enabled = VignetteCheck.IsChecked == true;
        _state.Update(s => s.Vignette.Enabled = enabled);
        UpdateSectionBodyState(VignetteBody, enabled);
    }

    private void VignetteOpacityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        VignetteOpacityVal.Text = $"{(int)Math.Round(e.NewValue)}%";
        _state.Update(s => s.Vignette.Opacity = Math.Clamp(e.NewValue / 100, 0, 1));
    }

    private void VignetteRadiusChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        VignetteRadiusVal.Text = $"{Math.Round(e.NewValue):F0}%";
        _state.Update(s => s.Vignette.Radius = Math.Round(e.NewValue, 1));
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
        if (key == WpfKey.Escape) { RefreshUi(); return; }
        if (IsModifierOnly(key)) { ToggleHotkeyBtn.Content = "Press a key…"; _capturingToggle = true; PreviewKeyDown += OnToggleHotkeyCaptureKeyDown; return; }

        var binding = new HotkeyBinding { Modifiers = WpfKeyboard.Modifiers, Key = key };
        if (binding.Equals(_state.Settings.Hotkeys.EmergencyOff))
        {
            _state.SetHotkeyStatus("Toggle and Emergency Off cannot share the same shortcut.", true);
            RefreshUi();
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
        if (key == WpfKey.Escape) { RefreshUi(); return; }
        if (IsModifierOnly(key)) { EmergencyHotkeyBtn.Content = "Press a key…"; _capturingEmergency = true; PreviewKeyDown += OnEmergencyHotkeyCaptureKeyDown; return; }

        var binding = new HotkeyBinding { Modifiers = WpfKeyboard.Modifiers, Key = key };
        if (binding.Equals(_state.Settings.Hotkeys.ToggleOverlay))
        {
            _state.SetHotkeyStatus("Toggle and Emergency Off cannot share the same shortcut.", true);
            RefreshUi();
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
    // Monitor
    // ──────────────────────────────────────────────────

    private void MonitorChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing || MonitorCombo.SelectedValue is not string monitorId) return;
        _state.Update(s => s.MonitorId = monitorId);
    }

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
                Visibility   = _state.ActivePresetName == preset.Name ? Visibility.Visible : Visibility.Collapsed,
                Tag          = preset.Name
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

            var isActive = _state.ActivePresetName == preset.Name;
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
