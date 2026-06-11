using Dot9.Models;
using Xunit;

namespace Dot9.Tests;

public class AppStateTests
{
    [Fact]
    public void ApplyPreset_PreservesOnboardingAndMonitor_ReplacesRest()
    {
        var state = new AppState();
        state.Update(s =>
        {
            s.HasSeenOnboarding = true;
            s.MonitorId = "Primary";
        });

        state.ApplyPreset(Presets.FastMotion);

        Assert.True(state.Settings.HasSeenOnboarding);
        Assert.Equal("Primary", state.Settings.MonitorId);
        Assert.Equal("Fast Motion", state.Settings.ActivePreset);
        Assert.Equal(Presets.FastMotion.CreateSettings().Dots.Opacity, state.Settings.Dots.Opacity);
    }

    [Fact]
    public void Update_RaisesSettingsChanged()
    {
        var state = new AppState();
        var raised = 0;
        state.SettingsChanged += (_, _) => raised++;

        state.Update(s => s.Dots.Opacity = 0.5);

        Assert.Equal(1, raised);
        Assert.Equal(0.5, state.Settings.Dots.Opacity);
    }

    [Fact]
    public void Update_RaisesHotkeysChanged_OnlyWhenHotkeysChange()
    {
        var state = new AppState();
        var hotkeyEvents = 0;
        state.HotkeysChanged += (_, _) => hotkeyEvents++;

        state.Update(s => s.Dots.Size = 12);
        Assert.Equal(0, hotkeyEvents);

        state.Update(s => s.Hotkeys.ToggleOverlay = new HotkeyBinding
        {
            Modifiers = System.Windows.Input.ModifierKeys.Control,
            Key = System.Windows.Input.Key.K
        });
        Assert.Equal(1, hotkeyEvents);
    }

    [Fact]
    public void EmergencyOff_DisablesOverlay()
    {
        var state = new AppState();
        state.SetOverlayEnabled(true);
        Assert.True(state.OverlayEnabled);

        state.EmergencyOff();

        Assert.False(state.OverlayEnabled);
    }

    [Fact]
    public void TuningAVisualProperty_MarksPresetCustom()
    {
        var state = new AppState();
        state.ApplyPreset(Presets.Gentle);

        state.DotOpacity = 0.5;

        Assert.Equal("Custom", state.ActivePresetName);
    }

    [Fact]
    public void ChangingMonitorOrAutoUpdate_DoesNotMarkCustom()
    {
        var state = new AppState();
        state.ApplyPreset(Presets.Gentle);

        state.MonitorId = "Primary";
        state.AutoUpdateEnabled = false;

        Assert.Equal("Gentle", state.ActivePresetName);
    }

    [Fact]
    public void ApplyPreset_AfterTuning_RestoresPresetName()
    {
        var state = new AppState();
        state.DotOpacity = 0.5;
        Assert.Equal("Custom", state.ActivePresetName);

        state.ApplyPreset(Presets.Vertigo);

        Assert.Equal("Vertigo", state.ActivePresetName);
    }

    [Fact]
    public void ApplyPreset_RaisesSettingsReplacing_WithOutgoingSettings()
    {
        var state = new AppState();
        state.Update(s => s.Dots.DotsPerEdge = 17);
        Dot9Settings? outgoing = null;
        state.SettingsReplacing += (_, s) => outgoing = s;

        state.ApplyPreset(Presets.Fps);

        Assert.NotNull(outgoing);
        Assert.Equal(17, outgoing!.Dots.DotsPerEdge);
        Assert.NotSame(outgoing, state.Settings);
    }
}
