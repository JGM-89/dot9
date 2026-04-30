using System.Windows;
using System.Windows.Controls;
using Dot9.Models;
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
        InitializeComponent();
        _state = state;
        DataContext = _state;

        ShapeCombo.ItemsSource = Enum.GetValues<DotShape>();
        EdgesCombo.ItemsSource = Enum.GetValues<EdgeSelection>();
        ColorCombo.ItemsSource = _palettes.Keys;

        BuildPresetCards();
        _state.PropertyChanged += (_, _) => RefreshUi();
        _state.SettingsChanged += (_, _) => RefreshUi();
        RefreshUi();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }

    private void RefreshUi()
    {
        _isRefreshing = true;
        HomePreview.Settings = _state.Settings;
        DotsPreview.Settings = _state.Settings;
        OverlayStatusText.Text = _state.OverlayStatusText;
        PresetModeText.Text = $"{_state.ActivePresetName} preset - {_state.ActiveModeName}";
        ToggleOverlayButton.Content = _state.OverlayEnabled ? "Turn overlay off" : "Turn overlay on";

        OpacitySlider.Value = Math.Round(_state.Settings.Dots.Opacity * 100);
        SizeSlider.Value = _state.Settings.Dots.Size;
        DistanceSlider.Value = _state.Settings.Dots.EdgeDistance;
        DotsEnabledCheck.IsChecked = _state.Settings.Dots.Enabled;
        ShapeCombo.SelectedItem = _state.Settings.Dots.Shape;
        EdgesCombo.SelectedItem = _state.Settings.Dots.Edges;
        DotsPerEdgeSlider.Value = _state.Settings.Dots.DotsPerEdge;
        ColorCombo.SelectedItem = _palettes.FirstOrDefault(p => p.Value.Equals(_state.Settings.Dots.Color, StringComparison.OrdinalIgnoreCase)).Key ?? "Soft Cyan";
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
                ShowDots(this, new RoutedEventArgs());
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

    private void DotsEnabledChanged(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing) return;
        _state.Update(settings => settings.Dots.Enabled = DotsEnabledCheck.IsChecked == true);
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

    private void ResetGentle(object sender, RoutedEventArgs e) => _state.ApplyPreset(Presets.Gentle);

    private void ShowHome(object sender, RoutedEventArgs e) => ShowOnly(HomeView);
    private void ShowPresets(object sender, RoutedEventArgs e) => ShowOnly(PresetsView);
    private void ShowDots(object sender, RoutedEventArgs e) => ShowOnly(DotsView);
    private void ShowSafety(object sender, RoutedEventArgs e) => ShowOnly(SafetyView);
    private void ShowAbout(object sender, RoutedEventArgs e) => ShowOnly(AboutView);

    private void ShowOnly(UIElement visible)
    {
        HomeView.Visibility = Visibility.Collapsed;
        PresetsView.Visibility = Visibility.Collapsed;
        DotsView.Visibility = Visibility.Collapsed;
        SafetyView.Visibility = Visibility.Collapsed;
        AboutView.Visibility = Visibility.Collapsed;
        visible.Visibility = Visibility.Visible;
    }
}
