namespace Dot9.Models;

public sealed record PresetDefinition(
    string Name,
    string ShortName,
    string Description,
    Func<Dot9Settings> CreateSettings);

public static class Presets
{
    public static readonly PresetDefinition Gentle = new(
        "Gentle Stable Dots",
        "Gentle",
        "The safest starting point. Small, fixed side dots that give your eyes a quiet reference frame.",
        () => new Dot9Settings
        {
            ActivePreset = "Gentle",
            MotionMode = MotionMode.StableAnchor,
            Dots = new DotSettings
            {
                DotsPerEdge = 9,
                Size = 8,
                Opacity = 0.32,
                Color = "#87D8E8",
                EdgeDistance = 34,
                Spacing = 44,
                Edges = EdgeSelection.LeftRight,
                AnimationEnabled = false
            },
            CentreAnchor = new CentreAnchorSettings { Enabled = false }
        });

    public static readonly PresetDefinition Fps = new(
        "FPS Comfort",
        "FPS",
        "For first-person games with fast camera turns.",
        () => new Dot9Settings
        {
            ActivePreset = "FPS",
            MotionMode = MotionMode.StableAnchor,
            Dots = new DotSettings
            {
                DotsPerEdge = 10,
                Size = 9,
                Opacity = 0.42,
                Color = "#F1F5F2",
                EdgeDistance = 30,
                Spacing = 40,
                Edges = EdgeSelection.LeftRight
            },
            CentreAnchor = new CentreAnchorSettings
            {
                Enabled = true,
                Size = 10,
                Opacity = 0.34,
                Color = "#F1F5F2",
                Shape = CentreAnchorShape.Ring
            }
        });

    public static readonly PresetDefinition Vertigo = new(
        "Vertigo Helper",
        "Vertigo",
        "For games with falling, flying, climbing, swimming, or camera roll.",
        () => new Dot9Settings
        {
            ActivePreset = "Vertigo",
            MotionMode = MotionMode.StableAnchor,
            Dots = new DotSettings
            {
                DotsPerEdge = 8,
                Size = 8,
                Opacity = 0.38,
                Color = "#B7E4C7",
                EdgeDistance = 38,
                Spacing = 48,
                Edges = EdgeSelection.AllEdges
            },
            CentreAnchor = new CentreAnchorSettings { Enabled = true, Opacity = 0.28, Shape = CentreAnchorShape.Ring }
        });

    public static readonly PresetDefinition FastMotion = new(
        "Motion Heavy",
        "Fast Motion",
        "For racing, parkour, wide-FOV movement, or fast camera motion.",
        () => new Dot9Settings
        {
            ActivePreset = "Fast Motion",
            MotionMode = MotionMode.StableAnchor,
            Dots = new DotSettings
            {
                DotsPerEdge = 12,
                Size = 10,
                Opacity = 0.48,
                Color = "#E9B86E",
                EdgeDistance = 32,
                Spacing = 36,
                Edges = EdgeSelection.LeftRight
            },
            CentreAnchor = new CentreAnchorSettings { Enabled = false }
        });

    public static readonly PresetDefinition Experimental = new(
        "Experimental Counter-Motion",
        "Experimental",
        "Dots move gently against mouse movement. Some players may prefer this for fast turns.",
        () => new Dot9Settings
        {
            ActivePreset = "Experimental",
            MotionMode = MotionMode.CounterMotion,
            Dots = new DotSettings
            {
                DotsPerEdge = 9,
                Size = 8,
                Opacity = 0.34,
                Color = "#B8A4FF",
                EdgeDistance = 34,
                Spacing = 44,
                Edges = EdgeSelection.LeftRight
            },
            CentreAnchor = new CentreAnchorSettings { Enabled = false }
        });

    public static IReadOnlyList<PresetDefinition> All { get; } =
    [
        Gentle,
        Fps,
        Vertigo,
        FastMotion,
        Experimental
    ];
}
