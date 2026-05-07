using System.Text.Json;
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
    public HotkeyBinding ToggleOverlay { get; set; } = HotkeyBinding.DefaultToggle;
    public HotkeyBinding EmergencyOff  { get; set; } = HotkeyBinding.DefaultEmergency;
}

[JsonConverter(typeof(HotkeyBindingJsonConverter))]
public struct HotkeyBinding : IEquatable<HotkeyBinding>
{
    public System.Windows.Input.ModifierKeys Modifiers { get; set; }
    public System.Windows.Input.Key          Key       { get; set; }

    public static HotkeyBinding DefaultToggle   => new() { Modifiers = System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Alt, Key = System.Windows.Input.Key.D };
    public static HotkeyBinding DefaultEmergency => new() { Key = System.Windows.Input.Key.F9 };

    public string DisplayName
    {
        get
        {
            var parts = new System.Collections.Generic.List<string>();
            if ((Modifiers & System.Windows.Input.ModifierKeys.Control) != 0) parts.Add("Ctrl");
            if ((Modifiers & System.Windows.Input.ModifierKeys.Alt)     != 0) parts.Add("Alt");
            if ((Modifiers & System.Windows.Input.ModifierKeys.Shift)   != 0) parts.Add("Shift");
            if ((Modifiers & System.Windows.Input.ModifierKeys.Windows) != 0) parts.Add("Win");
            var keyStr = Key switch
            {
                System.Windows.Input.Key.Back   => "Backspace",
                System.Windows.Input.Key.Return => "Enter",
                System.Windows.Input.Key.Escape => "Esc",
                System.Windows.Input.Key.Space  => "Space",
                _ => Key.ToString()
            };
            parts.Add(keyStr);
            return string.Join("+", parts);
        }
    }

    public bool IsEmpty => Key == System.Windows.Input.Key.None;

    public bool Equals(HotkeyBinding other) => Modifiers == other.Modifiers && Key == other.Key;
    public override bool Equals(object? obj) => obj is HotkeyBinding b && Equals(b);
    public override int GetHashCode() => HashCode.Combine(Modifiers, Key);
    public static bool operator ==(HotkeyBinding a, HotkeyBinding b) => a.Equals(b);
    public static bool operator !=(HotkeyBinding a, HotkeyBinding b) => !a.Equals(b);

    public static bool TryParse(string s, out HotkeyBinding binding)
    {
        binding = default;
        var parts = s.Split('+');
        var mods = System.Windows.Input.ModifierKeys.None;
        var key  = System.Windows.Input.Key.None;

        foreach (var part in parts)
        {
            switch (part.Trim())
            {
                case "Ctrl":  mods |= System.Windows.Input.ModifierKeys.Control; break;
                case "Alt":   mods |= System.Windows.Input.ModifierKeys.Alt;     break;
                case "Shift": mods |= System.Windows.Input.ModifierKeys.Shift;   break;
                case "Win":   mods |= System.Windows.Input.ModifierKeys.Windows; break;
                case "Backspace": key = System.Windows.Input.Key.Back;   break;
                case "Enter":     key = System.Windows.Input.Key.Return; break;
                case "Esc":       key = System.Windows.Input.Key.Escape; break;
                case "Space":     key = System.Windows.Input.Key.Space;  break;
                default:
                    if (Enum.TryParse<System.Windows.Input.Key>(part.Trim(), ignoreCase: true, out var k))
                        key = k;
                    break;
            }
        }

        if (key == System.Windows.Input.Key.None) return false;
        binding = new HotkeyBinding { Modifiers = mods, Key = key };
        return true;
    }
}

public sealed class HotkeyBindingJsonConverter : JsonConverter<HotkeyBinding>
{
    public override HotkeyBinding Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var s = reader.GetString() ?? "";
        return HotkeyBinding.TryParse(s, out var b) ? b : HotkeyBinding.DefaultToggle;
    }

    public override void Write(Utf8JsonWriter writer, HotkeyBinding value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.DisplayName);
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
