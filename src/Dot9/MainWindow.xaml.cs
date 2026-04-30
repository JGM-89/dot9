using System.Windows;
using System.Windows.Controls;
using Dot9.Models;
using Forms = System.Windows.Forms;
using WpfButton = System.Windows.Controls.Button;
using WpfOrientation = System.Windows.Controls.Orientation;

namespace Dot9;

public partial class MainWindow : Window
{
    private readonly AppState _state;
    private bool _isRefreshing;

    private readonly Dictionary<string, string> _palettes = new()
    {
        ["Soft Cyan"] = "#87D8E8",
        ["Warm White"] = "#F1F5F2",
        ["Muted Violet"] = "#B8A4FF",
        ["Gentle Amber"] = "#E9B86E",
        ["Soft Green"] = "#B7E4C7",
        ["Neutral Grey"] = "#B7C0C8"
    };

    public MainWindow(AppState state)
    {
        _state = state;
        _isRefreshing = true;
        InitializeComponent();
        DataContext = _state;

        ShapeCombo.ItemsSource = Enum.GetValues<DotShape>();
        EdgesCombo.ItemsSource = Enum.GetValues<EdgeSelection>();
        ColorCombo.ItemsSource = _palettes.Keys;
        CentreShapeCombo.ItemsSource = Enum.GetValues<CentreAnchorShape>();
        CentreColorCombo.ItemsSource = _palettes.Keys;
        HorizonStyleCombo.ItemsSource = Enum.GetValues<HorizonStyle>();
        HorizonColorCombo.ItemsSource = _palettes.Keys;
        ToggleHotkeyCombo.ItemsSource = Enum.GetValues<HotkeyChoice>();
        EmergencyHotkeyCombo.ItemsSource = Enum.GetValues<HotkeyChoice>();
        MonitorCombo.ItemsSource = BuildMonitorChoices();

        BuildPresetCards();
        _state.PropertyChanged += (_, _) => RefreshUi();
        _state.SettingsChanged += (_, _) => RefreshUi();
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
        {
            Hide();
        }
    }

    private void RefreshUi()
    {
        _isRefreshing = true;
        HomePreview.Settings = _state.Settings;
        HomePreview.InvalidateVisual();
        OverlayStatusText.Text = _state.OverlayStatusText;
        PresetModeText.Text = $"{_state.ActivePresetName} preset - {_state.ActiveModeName}";
        HotkeySummaryText.Text = $"{_state.Settings.Hotkeys.ToggleOverlay.GetDisplayName()} toggles, {_state.Settings.Hotkeys.EmergencyOff.GetDisplayName()} emergency off";
        ToggleOverlayButton.Content = _state.OverlayEnabled ? "Turn overlay off" : "Turn overlay on";

        OpacitySlider.Value = Math.Round(_state.Settings.Dots.Opacity * 100);
        SizeSlider.Value = _state.Settings.Dots.Size;
        DistanceSlider.Value = _state.Settings.Dots.EdgeDistance;
        CentreAnchorCheck.IsChecked = _state.Settings.CentreAnchor.Enabled;
        CentreShapeCombo.SelectedItem = _state.Settings.CentreAnchor.Shape;
        CentreSizeSlider.Value = _state.Settings.CentreAnchor.Size;
        CentreOpacitySlider.Value = Math.Round(_state.Settings.CentreAnchor.Opacity * 100);
        CentreColorCombo.SelectedItem = _palettes.FirstOrDefault(p => p.Value.Equals(_state.Settings.CentreAnchor.Color, StringComparison.OrdinalIgnoreCase)).Key ?? "Warm White";
        HorizonCheck.IsChecked = _state.Settings.Horizon.Enabled;
        HorizonStyleCombo.SelectedItem = _state.Settings.Horizon.Style;
        HorizonPositionSlider.Value = _state.Settings.Horizon.VerticalPosition;
        HorizonWidthSlider.Value = _state.Settings.Horizon.Width;
        HorizonOpacitySlider.Value = Math.Round(_state.Settings.Horizon.Opacity * 100);
        HorizonColorCombo.SelectedItem = _palettes.FirstOrDefault(p => p.Value.Equals(_state.Settings.Horizon.Color, StringComparison.OrdinalIgnoreCase)).Key ?? "Soft Cyan";
        VignetteCheck.IsChecked = _state.Settings.Vignette.Enabled;
        VignetteStrengthSlider.Value = _state.Settings.Vignette.Strength;
        VignetteRadiusSlider.Value = _state.Settings.Vignette.Radius;
        VignetteOpacitySlider.Value = Math.Round(_state.Settings.Vignette.Opacity * 100);
        ShapeCombo.SelectedItem = _state.Settings.Dots.Shape;
        EdgesCombo.SelectedItem = _state.Settings.Dots.Edges;
        DotsPerEdgeSlider.Value = _state.Settings.Dots.DotsPerEdge;
        ColorCombo.SelectedItem = _palettes.FirstOrDefault(p => p.Value.Equals(_state.Settings.Dots.Color, StringComparison.OrdinalIgnoreCase)).Key ?? "Soft Cyan";
        ToggleHotkeyCombo.SelectedItem = _state.Settings.Hotkeys.ToggleOverlay;
        EmergencyHotkeyCombo.SelectedItem = _state.Settings.Hotkeys.EmergencyOff;
        MonitorCombo.SelectedValue = _state.Settings.MonitorId;
        _isRefreshing = false;
    }

    private void BuildPresetCards()
    {
        PresetGrid.Children.Clear();
        foreach (var preset in Presets.All)
        {
            var card = new Border
            {
                Style = (Style)FindResource("PanelCard"),
                Margin = new Thickness(0, 0, 16, 16)
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = preset.Name, FontSize = 21, FontWeight = FontWeights.SemiBold });
            stack.Children.Add(new TextBlock
            {
                Text = preset.Description,
                Foreground = (System.Windows.Media.Brush)FindResource("MutedInkBrush"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 8, 0, 12)
            });
            stack.Children.Add(new PreviewSurface
            {
                Settings = preset.CreateSettings(),
                Height = 150,
                Margin = new Thickness(0, 0, 0, 14)
            });

            var buttons = new StackPanel { Orientation = WpfOrientation.Horizontal };
            var useButton = new WpfButton { Content = "Use preset", Style = (Style)FindResource("PrimaryButton"), Margin = new Thickness(0, 0, 10, 0) };
            useButton.Click += (_, _) => _state.ApplyPreset(preset);
            var customizeButton = new WpfButton { Content = "Customise" };
            customizeButton.Click += (_, _) =>
            {
                _state.ApplyPreset(preset);
                ShowHome(this, new RoutedEventArgs());
            };

            buttons.Children.Add(useButton);
            buttons.Children.Add(customizeButton);
            stack.Children.Add(buttons);
            card.Child = stack;
            PresetGrid.Children.Add(card);
        }
    }

    private void ToggleOverlay(object sender, RoutedEventArgs e) => _state.ToggleOverlay();

    private void EmergencyOff(object sender, RoutedEventArgs e) => _state.EmergencyOff();

    private void OpacityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        _state.Update(settings => settings.Dots.Opacity = Math.Clamp(e.NewValue / 100, 0, 1));
    }

    private void DotSizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        _state.Update(settings => settings.Dots.Size = Math.Round(e.NewValue, 1));
    }

    private void DistanceChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        _state.Update(settings => settings.Dots.EdgeDistance = Math.Round(e.NewValue, 1));
    }

    private void DotsPerEdgeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        _state.Update(settings => settings.Dots.DotsPerEdge = (int)Math.Round(e.NewValue));
    }

    private void CentreAnchorChanged(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing) return;
        _state.Update(settings => settings.CentreAnchor.Enabled = CentreAnchorCheck.IsChecked == true);
    }

    private void CentreShapeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing || CentreShapeCombo.SelectedItem is not CentreAnchorShape shape) return;
        _state.Update(settings => settings.CentreAnchor.Shape = shape);
    }

    private void CentreSizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        _state.Update(settings => settings.CentreAnchor.Size = Math.Round(e.NewValue, 1));
    }

    private void CentreOpacityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        _state.Update(settings => settings.CentreAnchor.Opacity = Math.Clamp(e.NewValue / 100, 0, 1));
    }

    private void CentreColorChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing || CentreColorCombo.SelectedItem is not string key || !_palettes.TryGetValue(key, out var color)) return;
        _state.Update(settings => settings.CentreAnchor.Color = color);
    }

    private void HorizonChanged(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing) return;
        _state.Update(settings => settings.Horizon.Enabled = HorizonCheck.IsChecked == true);
    }

    private void HorizonStyleChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing || HorizonStyleCombo.SelectedItem is not HorizonStyle style) return;
        _state.Update(settings => settings.Horizon.Style = style);
    }

    private void HorizonPositionChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        _state.Update(settings => settings.Horizon.VerticalPosition = Math.Round(e.NewValue, 1));
    }

    private void HorizonWidthChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        _state.Update(settings => settings.Horizon.Width = Math.Round(e.NewValue, 1));
    }

    private void HorizonOpacityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        _state.Update(settings => settings.Horizon.Opacity = Math.Clamp(e.NewValue / 100, 0, 1));
    }

    private void HorizonColorChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing || HorizonColorCombo.SelectedItem is not string key || !_palettes.TryGetValue(key, out var color)) return;
        _state.Update(settings => settings.Horizon.Color = color);
    }

    private void VignetteChanged(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing) return;
        _state.Update(settings => settings.Vignette.Enabled = VignetteCheck.IsChecked == true);
    }

    private void VignetteStrengthChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        _state.Update(settings => settings.Vignette.Strength = Math.Round(e.NewValue, 1));
    }

    private void VignetteRadiusChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        _state.Update(settings => settings.Vignette.Radius = Math.Round(e.NewValue, 1));
    }

    private void VignetteOpacityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isRefreshing) return;
        _state.Update(settings => settings.Vignette.Opacity = Math.Clamp(e.NewValue / 100, 0, 1));
    }

    private void ShapeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing || ShapeCombo.SelectedItem is not DotShape shape) return;
        _state.Update(settings => settings.Dots.Shape = shape);
    }

    private void EdgesChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing || EdgesCombo.SelectedItem is not EdgeSelection edges) return;
        _state.Update(settings => settings.Dots.Edges = edges);
    }

    private void ColorChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing || ColorCombo.SelectedItem is not string key || !_palettes.TryGetValue(key, out var color)) return;
        _state.Update(settings => settings.Dots.Color = color);
    }

    private void MonitorChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing || MonitorCombo.SelectedValue is not string monitorId) return;
        _state.Update(settings => settings.MonitorId = monitorId);
    }

    private void ToggleHotkeyChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing || ToggleHotkeyCombo.SelectedItem is not HotkeyChoice choice) return;
        _state.Update(settings => settings.Hotkeys.ToggleOverlay = choice);
    }

    private void EmergencyHotkeyChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing || EmergencyHotkeyCombo.SelectedItem is not HotkeyChoice choice) return;
        _state.Update(settings => settings.Hotkeys.EmergencyOff = choice);
    }

    private void ResetGentle(object sender, RoutedEventArgs e) => _state.ApplyPreset(Presets.Gentle);

    private void ShowHome(object sender, RoutedEventArgs e) => ShowOnly(HomeView);
    private void ShowPresets(object sender, RoutedEventArgs e) => ShowOnly(PresetsView);
    private void ShowSafety(object sender, RoutedEventArgs e) => ShowOnly(SafetyView);
    private void ShowAbout(object sender, RoutedEventArgs e) => ShowOnly(AboutView);

    private void ShowOnly(UIElement visible)
    {
        HomeView.Visibility = Visibility.Collapsed;
        PresetsView.Visibility = Visibility.Collapsed;
        SafetyView.Visibility = Visibility.Collapsed;
        AboutView.Visibility = Visibility.Collapsed;
        visible.Visibility = Visibility.Visible;
    }

    private static IReadOnlyList<MonitorChoice> BuildMonitorChoices()
    {
        var choices = new List<MonitorChoice>
        {
            new("All", "All monitors"),
            new("Primary", "Primary monitor")
        };

        var index = 1;
        foreach (var screen in Forms.Screen.AllScreens)
        {
            choices.Add(new MonitorChoice(screen.DeviceName, $"Monitor {index}: {screen.Bounds.Width}x{screen.Bounds.Height}{(screen.Primary ? " primary" : "")}"));
            index++;
        }

        return choices;
    }

    private sealed record MonitorChoice(string Id, string Label)
    {
        public override string ToString() => Label;
    }
}
