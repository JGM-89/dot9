using System.Windows;
using System.Windows.Controls;
using Dot9.Controls;
using Dot9.Models;
using WpfColor = System.Windows.Media.Color;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using WpfKey = System.Windows.Input.Key;

namespace Dot9.Windows;

public partial class OnboardingOverlay : Window
{
    private readonly AppState _state;
    private int _step;

    private static readonly (string Title, string Body)[] Steps =
    [
        (
            "Hi. This is dot[9].",
            "dot[9] puts subtle visual anchors on your screen — stable dots, a centre point, a horizon line, and a gentle vignette — so your eyes have somewhere fixed to land during fast camera movement.\n\nIt lives as a transparent overlay on top of your game. It doesn't interact with the game at all."
        ),
        (
            "Pick a starting point",
            "The Presets screen has four conservative starting points: Gentle, FPS, Vertigo, and Fast Motion.\n\nStart with Gentle — the lowest-impact option. Apply a preset and see how it feels before adjusting anything."
        ),
        (
            "Tune to your eyes",
            "Every setting is adjustable from the Tune screen. Change opacity, size, distance, shape, and which edges show dots.\n\nMake small changes. Give each change a few minutes of play before judging it."
        ),
        (
            "Two safety things",
            "Press F9 (Emergency Off) to remove the overlay instantly if you feel unwell. You can change this shortcut in Hotkeys.\n\ndot[9] is a comfort tool, not a medical treatment. Stop playing if discomfort gets strong."
        )
    ];

    public OnboardingOverlay(AppState state)
    {
        _state = state;
        InitializeComponent();
        SetStep(0);
    }

    private void SetStep(int step)
    {
        _step = Math.Clamp(step, 0, Steps.Length - 1);
        UpdateProgressSegments();
        RenderStepContent();

        BackBtn.IsEnabled = _step > 0;
        NextBtn.Content   = _step == Steps.Length - 1 ? "Start" : "Next →";
    }

    private void UpdateProgressSegments()
    {
        var accent = new System.Windows.Media.SolidColorBrush(WpfColor.FromRgb(0xE9, 0xB8, 0x6E));
        var dim    = new System.Windows.Media.SolidColorBrush(WpfColor.FromArgb(0x1F, 0xFF, 0xF0, 0xD2));

        Seg0.Background = _step >= 0 ? accent : dim;
        Seg1.Background = _step >= 1 ? accent : dim;
        Seg2.Background = _step >= 2 ? accent : dim;
        Seg3.Background = _step >= 3 ? accent : dim;
    }

    private void RenderStepContent()
    {
        StepContent.Children.Clear();
        var (title, body) = Steps[_step];

        if (_step == 0)
        {
            StepContent.Children.Add(new WordmarkControl
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 24),
                LayoutTransform = new System.Windows.Media.ScaleTransform(2.4, 2.4)
            });
        }
        else if (_step == 3)
        {
            var chip = new Border
            {
                Background      = new System.Windows.Media.SolidColorBrush(WpfColor.FromArgb(0x1F, 0xE9, 0xB8, 0x6E)),
                BorderBrush     = new System.Windows.Media.SolidColorBrush(WpfColor.FromArgb(0x33, 0xE9, 0xB8, 0x6E)),
                BorderThickness = new Thickness(1),
                CornerRadius    = new CornerRadius(8),
                Padding         = new Thickness(24, 14, 24, 14),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Margin          = new Thickness(0, 0, 0, 24)
            };
            chip.Child = new TextBlock
            {
                Text       = "F9",
                FontSize   = 36,
                FontWeight = FontWeights.Bold,
                FontFamily = new WpfFontFamily("JetBrains Mono, Cascadia Code, Consolas"),
                Foreground = new System.Windows.Media.SolidColorBrush(WpfColor.FromRgb(0xE9, 0xB8, 0x6E)),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };
            StepContent.Children.Add(chip);
        }

        StepContent.Children.Add(new TextBlock
        {
            Text         = title,
            FontSize     = 22,
            FontWeight   = FontWeights.SemiBold,
            Foreground   = new System.Windows.Media.SolidColorBrush(WpfColor.FromRgb(0xEF, 0xEA, 0xE0)),
            TextWrapping = TextWrapping.Wrap,
            Margin       = new Thickness(0, 0, 0, 14)
        });

        StepContent.Children.Add(new TextBlock
        {
            Text         = body,
            FontSize     = 14,
            Foreground   = new System.Windows.Media.SolidColorBrush(WpfColor.FromRgb(0x8A, 0x83, 0x77)),
            TextWrapping = TextWrapping.Wrap,
            LineHeight   = 22
        });
    }

    private void OnNext(object sender, RoutedEventArgs e)
    {
        if (_step == Steps.Length - 1)
            Close();
        else
            SetStep(_step + 1);
    }

    private void OnBack(object sender, RoutedEventArgs e) => SetStep(_step - 1);

    private void OnSkip(object sender, RoutedEventArgs e) => Close();

    private void OnKeyDown(object sender, WpfKeyEventArgs e)
    {
        if (e.Key == WpfKey.Escape)
        {
            Close();
            e.Handled = true;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _state.Update(s => s.HasSeenOnboarding = true);
        _state.ShowOnboarding = false;
        base.OnClosed(e);
    }
}
