using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using WpfUserControl = System.Windows.Controls.UserControl;
using WpfBrush = System.Windows.Media.Brush;
using WpfColor = System.Windows.Media.Color;

namespace Dot9.Controls;

public partial class SwatchRow : WpfUserControl
{
    public static readonly DependencyProperty PaletteProperty =
        DependencyProperty.Register(nameof(Palette), typeof(IReadOnlyDictionary<string, string>), typeof(SwatchRow),
            new PropertyMetadata(null, OnPaletteChanged));

    public static readonly DependencyProperty SelectedColorProperty =
        DependencyProperty.Register(nameof(SelectedColor), typeof(string), typeof(SwatchRow),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

    public event EventHandler<string?>? SelectedColorChanged;

    private bool _updating;

    public SwatchRow()
    {
        InitializeComponent();
    }

    public IReadOnlyDictionary<string, string>? Palette
    {
        get => (IReadOnlyDictionary<string, string>?)GetValue(PaletteProperty);
        set => SetValue(PaletteProperty, value);
    }

    public string? SelectedColor
    {
        get => (string?)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    private static void OnPaletteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SwatchRow)d).RebuildSwatches();
    }

    private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SwatchRow)d).UpdateActiveState();
    }

    private void RebuildSwatches()
    {
        SwatchPanel.Children.Clear();
        if (Palette is null) return;

        foreach (var (name, hex) in Palette)
        {
            var fill = TryParseColor(hex, out var color)
                ? (WpfBrush)new SolidColorBrush(color)
                : (WpfBrush)System.Windows.Media.Brushes.Gray;

            var btn = new ToggleButton
            {
                Tag = hex,
                Width = 32,
                Height = 32,
                Margin = new Thickness(0, 0, 6, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                ToolTip = name
            };
            btn.Template = BuildSwatchTemplate(fill);
            btn.Click += OnSwatchClick;
            SwatchPanel.Children.Add(btn);
        }

        UpdateActiveState();
    }

    private void OnSwatchClick(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton btn && btn.Tag is string hex)
        {
            _updating = true;
            SelectedColor = hex;
            _updating = false;
            SelectedColorChanged?.Invoke(this, hex);
            UpdateActiveState();
        }
    }

    private void UpdateActiveState()
    {
        if (_updating) return;
        foreach (ToggleButton btn in SwatchPanel.Children)
        {
            btn.IsChecked = btn.Tag is string hex &&
                            string.Equals(hex, SelectedColor, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static ControlTemplate BuildSwatchTemplate(WpfBrush fill)
    {
        var template = new ControlTemplate(typeof(ToggleButton));
        var factory = new FrameworkElementFactory(typeof(Border));
        factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
        factory.SetValue(Border.PaddingProperty, new Thickness(3));
        factory.SetValue(Border.BackgroundProperty, System.Windows.Media.Brushes.Transparent);

        var innerFactory = new FrameworkElementFactory(typeof(Border));
        innerFactory.SetValue(Border.BackgroundProperty, fill);
        innerFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
        factory.AppendChild(innerFactory);

        template.VisualTree = factory;

        var checkedTrigger = new Trigger { Property = ToggleButton.IsCheckedProperty, Value = true };
        checkedTrigger.Setters.Add(new Setter
        {
            TargetName = null,
            Property = Border.BorderBrushProperty,
            Value = new SolidColorBrush(WpfColor.FromRgb(0xE9, 0xB8, 0x6E))
        });
        checkedTrigger.Setters.Add(new Setter
        {
            Property = Border.BorderThicknessProperty,
            Value = new Thickness(1.5)
        });
        template.Triggers.Add(checkedTrigger);

        return template;
    }

    private static bool TryParseColor(string hex, out WpfColor color)
    {
        try
        {
            color = (WpfColor)System.Windows.Media.ColorConverter.ConvertFromString(hex)!;
            return true;
        }
        catch
        {
            color = System.Windows.Media.Colors.Gray;
            return false;
        }
    }
}
