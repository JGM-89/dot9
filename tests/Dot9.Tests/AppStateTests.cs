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
}
