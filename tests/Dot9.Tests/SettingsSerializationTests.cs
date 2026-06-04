using System.Text.Json;
using System.Windows.Input;
using Dot9.Models;
using Xunit;

namespace Dot9.Tests;

public class SettingsSerializationTests
{
    [Fact]
    public void Settings_RoundTrip_PreservesValues()
    {
        var original = Dot9Settings.CreateDefault();
        original.MonitorId = "Primary";
        original.Dots.Opacity = 0.47;
        original.Dots.Shape = DotShape.Ring;
        original.Dots.Edges = EdgeSelection.AllEdges;
        original.Horizon.Enabled = true;
        original.Horizon.Style = HorizonStyle.Segmented;
        original.Hotkeys.ToggleOverlay = new HotkeyBinding { Modifiers = ModifierKeys.Control | ModifierKeys.Shift, Key = Key.T };

        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<Dot9Settings>(json);

        Assert.NotNull(restored);
        Assert.Equal(original.MonitorId, restored!.MonitorId);
        Assert.Equal(original.Dots.Opacity, restored.Dots.Opacity);
        Assert.Equal(original.Dots.Shape, restored.Dots.Shape);
        Assert.Equal(original.Dots.Edges, restored.Dots.Edges);
        Assert.True(restored.Horizon.Enabled);
        Assert.Equal(HorizonStyle.Segmented, restored.Horizon.Style);
        Assert.Equal(original.Hotkeys.ToggleOverlay, restored.Hotkeys.ToggleOverlay);
    }

    [Fact]
    public void Enums_SerializeAsStrings()
    {
        var settings = Dot9Settings.CreateDefault();
        settings.Dots.Shape = DotShape.SoftGlow;

        var json = JsonSerializer.Serialize(settings);

        Assert.Contains("SoftGlow", json);
    }

    [Fact]
    public void HotkeyBinding_SerializesAsDisplayName()
    {
        var settings = Dot9Settings.CreateDefault();
        settings.Hotkeys.EmergencyOff = new HotkeyBinding { Key = Key.F9 };

        var json = JsonSerializer.Serialize(settings);

        Assert.Contains("\"F9\"", json);
    }
}
