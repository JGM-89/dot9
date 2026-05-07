using System.Text.Json.Serialization;

namespace Dot9.Models;

public sealed class Dot9Settings
{
    public string Version { get; set; } = "1.0.0";
    public string ActivePreset { get; set; } = "Gentle";
    public MotionMode MotionMode { get; set; } = MotionMode.StableAnchor;
    public bool StartOverlayEnabled { get; set; }
    public bool AllAnimationsEnabled { get; set; } = true;
    public string MonitorId { get; set; } = "All";
    public HotkeySettings Hotkeys { get; set; } = new();
    public DotSettings Dots { get; set; } = new();
    public CentreAnchorSettings CentreAnchor { get; set; } = new();
    public HorizonSettings Horizon { get; set; } = new();
    public VignetteSettings Vignette { get; set; } = new();

    public bool HasSeenOnboarding { get; set; }

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
        MotionMode.CounterMotion => "Counter-Motion (research)",
        MotionMode.MotionEcho => "Motion Echo (research)",
        MotionMode.BreathingStatic => "Breathing Static",
        MotionMode.AdaptiveComfort => "Adaptive Comfort (research)",
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

public sealed class HorizonSettings
{
    public bool Enabled { get; set; }
    public double VerticalPosition { get; set; } = 54;
    public double Width { get; set; } = 56;
    public double Thickness { get; set; } = 1.2;
    public string Color { get; set; } = "#87D8E8";
    public double Opacity { get; set; } = 0.28;
    public double CentreGap { get; set; } = 18;
    public HorizonStyle Style { get; set; } = HorizonStyle.FullLine;
}

public sealed class VignetteSettings
{
    public bool Enabled { get; set; }
    public string Color { get; set; } = "#020609";
    public double Opacity { get; set; } = 0.26;
    public double Radius { get; set; } = 58;
    public double Strength { get; set; } = 55;
}

public sealed class HotkeySettings
{
    public HotkeyChoice ToggleOverlay { get; set; } = HotkeyChoice.CtrlAltD;
    public HotkeyChoice EmergencyOff { get; set; } = HotkeyChoice.F9;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HotkeyChoice
{
    CtrlAltD,
    CtrlAltO,
    F8,
    F9,
    F10,
    F12,
    CtrlAltBackspace
}

public static class HotkeyChoiceExtensions
{
    public static string GetDisplayName(this HotkeyChoice choice) => choice switch
    {
        HotkeyChoice.CtrlAltD => "Ctrl+Alt+D",
        HotkeyChoice.CtrlAltO => "Ctrl+Alt+O",
        HotkeyChoice.F8 => "F8",
        HotkeyChoice.F9 => "F9",
        HotkeyChoice.F10 => "F10",
        HotkeyChoice.F12 => "F12",
        HotkeyChoice.CtrlAltBackspace => "Ctrl+Alt+Backspace",
        _ => choice.ToString()
    };
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CentreAnchorShape
{
    Dot,
    Ring,
    Cross
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HorizonStyle
{
    SideTicks,
    Segmented,
    FullLine
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
