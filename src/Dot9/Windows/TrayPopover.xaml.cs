using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Dot9.Models;
using WpfColor = System.Windows.Media.Color;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using WpfKey = System.Windows.Input.Key;
using WpfRadioButton = System.Windows.Controls.RadioButton;

namespace Dot9.Windows;

public partial class TrayPopover : Window
{
    private readonly AppState _state;
    private readonly Action _openSettings;
    private bool _refreshing;

    public TrayPopover(AppState state, Action openSettings)
    {
        _state = state;
        _openSettings = openSettings;
        InitializeComponent();
        BuildPresetRows();
        Refresh();

        _state.PropertyChanged += (_, _) => Dispatcher.InvokeAsync(Refresh);
    }

    private void Refresh()
    {
        _refreshing = true;
        TrayOverlayToggle.IsChecked = _state.OverlayEnabled;
        TrayHotkeyChip.Text = _state.Settings.Hotkeys.ToggleOverlay.GetDisplayName();
        _refreshing = false;
    }

    private void BuildPresetRows()
    {
        PresetRows.Children.Clear();
        foreach (var preset in Presets.All)
        {
            var row = new Grid { Margin = new Thickness(0, 0, 0, 6) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var radio = new WpfRadioButton
            {
                GroupName         = "TrayPresetGroup",
                IsChecked         = _state.ActivePresetName == preset.Name,
                VerticalAlignment = VerticalAlignment.Center,
                Margin            = new Thickness(0, 0, 8, 0),
                Style             = BuildRadioStyle()
            };
            radio.Checked += (_, _) =>
            {
                if (!_refreshing) _state.ApplyPreset(preset);
            };
            Grid.SetColumn(radio, 0);

            var label = new TextBlock
            {
                Text              = preset.Name,
                FontSize          = 13,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor            = System.Windows.Input.Cursors.Hand
            };
            label.MouseDown += (_, _) =>
            {
                _state.ApplyPreset(preset);
                BuildPresetRows();
            };
            Grid.SetColumn(label, 1);

            row.Children.Add(radio);
            row.Children.Add(label);
            PresetRows.Children.Add(row);
        }
    }

    private static Style BuildRadioStyle()
    {
        var style = new Style(typeof(WpfRadioButton));
        var template = new ControlTemplate(typeof(WpfRadioButton));
        var factory = new FrameworkElementFactory(typeof(Ellipse));
        factory.SetValue(FrameworkElement.WidthProperty, 10.0);
        factory.SetValue(FrameworkElement.HeightProperty, 10.0);
        factory.SetValue(Shape.FillProperty, new SolidColorBrush(WpfColor.FromArgb(0x1F, 0xFF, 0xF0, 0xD2)));
        factory.SetValue(Shape.StrokeProperty, new SolidColorBrush(WpfColor.FromArgb(0x3F, 0xFF, 0xF0, 0xD2)));
        factory.SetValue(Shape.StrokeThicknessProperty, 1.0);
        template.VisualTree = factory;
        var trigger = new Trigger { Property = ToggleButton.IsCheckedProperty, Value = true };
        trigger.Setters.Add(new Setter
        {
            TargetName = null,
            Property   = Shape.FillProperty,
            Value      = new SolidColorBrush(WpfColor.FromRgb(0xE9, 0xB8, 0x6E))
        });
        template.Triggers.Add(trigger);
        style.Setters.Add(new Setter { Property = System.Windows.Controls.Control.TemplateProperty, Value = template });
        return style;
    }

    private void TrayOverlayChecked(object sender, RoutedEventArgs e)
    {
        if (!_refreshing) _state.SetOverlayEnabled(true);
    }

    private void TrayOverlayUnchecked(object sender, RoutedEventArgs e)
    {
        if (!_refreshing) _state.SetOverlayEnabled(false);
    }

    private void OpenSettings(object sender, RoutedEventArgs e)
    {
        Close();
        _openSettings();
    }

    private void TrayEmergencyOff(object sender, RoutedEventArgs e)
    {
        _state.EmergencyOff();
        Close();
    }

    private void OnDeactivated(object sender, EventArgs e) => Close();

    private void OnKeyDown(object sender, WpfKeyEventArgs e)
    {
        if (e.Key == WpfKey.Escape)
        {
            Close();
            e.Handled = true;
        }
    }
}
