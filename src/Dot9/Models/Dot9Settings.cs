using System.Text.Json.Serialization;

namespace Dot9.Models;

public sealed class Dot9Settings
{
    public string Version { get; set; } = "0.1.0";
    public string ActivePreset { get; set; } = "Gentle";
    public MotionMode MotionMode { get; set; } = MotionMode.StableAnchor;
    public bool StartOverlayEnabled { get; set; }
    public bool AllAnimationsEnabled { get; set; } = true;
    public DotSettings Dots { get; set; } = new();
    public CentreAnchorSettings CentreAnchor { get; set; } = new();

    public static Dot9Settings CreateDefault() => Presets.Gentle.CreateSettings();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MotionMode
{
    StableAnchor,
    CounterMotion,
    MotionEcho,
    BreathingStatic,
    AdaptiveComfort
}

public static class MotionModeExtensions
{
    public static string GetDisplayName(this MotionMode mode) => mode switch
    {
        MotionMode.StableAnchor => "Stable Anchor",
        MotionMode.CounterMotion => "Counter-Motion (experimental)",
        MotionMode.MotionEcho => "Motion Echo (experimental)",
        MotionMode.BreathingStatic => "Breathing Static",
        MotionMode.AdaptiveComfort => "Adaptive Comfort (experimental)",
        _ => "Stable Anchor"
    };
}

public sealed class DotSettings
{
    public bool Enabled { get; set; } = true;
    public int DotsPerEdge { get; set; } = 9;
    public double Size { get; set; } = 8;
    public string Color { get; set; } = "#87D8E8";
    public double Opacity { get; set; } = 0.32;
    public double Spacing { get; set; } = 44;
    public double EdgeDistance { get; set; } = 34;
    public double CornerExclusion { get; set; } = 92;
    public DotShape Shape { get; set; } = DotShape.Circle;
    public EdgeSelection Edges { get; set; } = EdgeSelection.LeftRight;
    public bool Symmetric { get; set; } = true;
    public bool AlternatingSizes { get; set; }
    public bool Randomized { get; set; }
    public bool AnimationEnabled { get; set; }
}

public sealed class CentreAnchorSettings
{
    public bool Enabled { get; set; }
    public double Size { get; set; } = 10;
    public string Color { get; set; } = "#F1F5F2";
    public double Opacity { get; set; } = 0.36;
    public double StrokeWidth { get; set; } = 1.4;
    public CentreAnchorShape Shape { get; set; } = CentreAnchorShape.Ring;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CentreAnchorShape
{
    Dot,
    Ring,
    Cross
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DotShape
{
    Circle,
    Pill,
    Ring,
    SoftGlow
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EdgeSelection
{
    LeftOnly,
    RightOnly,
    TopOnly,
    BottomOnly,
    LeftRight,
    TopBottom,
    AllEdges
}
