using System.Windows.Input;
using Dot9.Models;
using Xunit;

namespace Dot9.Tests;

public class HotkeyBindingTests
{
    [Fact]
    public void TryParse_ModifiersAndKey_Succeeds()
    {
        Assert.True(HotkeyBinding.TryParse("Ctrl+Alt+D", out var binding));
        Assert.Equal(ModifierKeys.Control | ModifierKeys.Alt, binding.Modifiers);
        Assert.Equal(Key.D, binding.Key);
    }

    [Fact]
    public void TryParse_TrimsWhitespaceAroundParts()
    {
        Assert.True(HotkeyBinding.TryParse(" Ctrl + Shift + f9 ", out var binding));
        Assert.Equal(ModifierKeys.Control | ModifierKeys.Shift, binding.Modifiers);
        Assert.Equal(Key.F9, binding.Key); // key name parse is case-insensitive
    }

    [Theory]
    [InlineData("Backspace", Key.Back)]
    [InlineData("Enter", Key.Return)]
    [InlineData("Esc", Key.Escape)]
    [InlineData("Space", Key.Space)]
    public void TryParse_SpecialKeyNames_MapCorrectly(string input, Key expected)
    {
        Assert.True(HotkeyBinding.TryParse(input, out var binding));
        Assert.Equal(expected, binding.Key);
        Assert.Equal(ModifierKeys.None, binding.Modifiers);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Ctrl+Alt")]
    [InlineData("nonsense")]
    public void TryParse_NoUsableKey_Fails(string input)
    {
        Assert.False(HotkeyBinding.TryParse(input, out _));
    }

    [Fact]
    public void DisplayName_RendersModifierOrderAndKey()
    {
        var binding = new HotkeyBinding { Modifiers = ModifierKeys.Control | ModifierKeys.Alt, Key = Key.D };
        Assert.Equal("Ctrl+Alt+D", binding.DisplayName);
    }

    [Theory]
    [InlineData("Ctrl+Alt+D")]
    [InlineData("Shift+Esc")]
    [InlineData("Ctrl+Shift+Backspace")]
    [InlineData("F9")]
    public void TryParse_Then_DisplayName_RoundTrips(string text)
    {
        Assert.True(HotkeyBinding.TryParse(text, out var binding));
        Assert.Equal(text, binding.DisplayName);
    }

    [Fact]
    public void Equality_ComparesModifiersAndKey()
    {
        var a = new HotkeyBinding { Modifiers = ModifierKeys.Control, Key = Key.D };
        var b = new HotkeyBinding { Modifiers = ModifierKeys.Control, Key = Key.D };
        var c = new HotkeyBinding { Modifiers = ModifierKeys.Alt, Key = Key.D };

        Assert.True(a == b);
        Assert.True(a != c);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    // v1.0.0 stored hotkeys as HotkeyChoice enum names rather than "Ctrl+Alt+D" strings.
    [Theory]
    [InlineData("CtrlAltD", ModifierKeys.Control | ModifierKeys.Alt, Key.D)]
    [InlineData("CtrlAltO", ModifierKeys.Control | ModifierKeys.Alt, Key.O)]
    [InlineData("CtrlAltBackspace", ModifierKeys.Control | ModifierKeys.Alt, Key.Back)]
    [InlineData("F8", ModifierKeys.None, Key.F8)]
    [InlineData("F12", ModifierKeys.None, Key.F12)]
    public void TryParse_LegacyV100Names_Succeeds(string legacy, ModifierKeys mods, Key key)
    {
        Assert.True(HotkeyBinding.TryParse(legacy, out var binding));
        Assert.Equal(mods, binding.Modifiers);
        Assert.Equal(key, binding.Key);
    }

    [Fact]
    public void Deserialize_V100Settings_KeepsDistinctHotkeys()
    {
        const string json = """{ "Hotkeys": { "ToggleOverlay": "CtrlAltO", "EmergencyOff": "CtrlAltBackspace" } }""";

        var settings = System.Text.Json.JsonSerializer.Deserialize<Dot9Settings>(json)!;

        Assert.Equal(new HotkeyBinding { Modifiers = ModifierKeys.Control | ModifierKeys.Alt, Key = Key.O }, settings.Hotkeys.ToggleOverlay);
        Assert.Equal(new HotkeyBinding { Modifiers = ModifierKeys.Control | ModifierKeys.Alt, Key = Key.Back }, settings.Hotkeys.EmergencyOff);
    }

    [Fact]
    public void Deserialize_UnparseableHotkeys_FallBackToTheirOwnDefaults()
    {
        const string json = """{ "Hotkeys": { "ToggleOverlay": "???", "EmergencyOff": 42 } }""";

        var settings = System.Text.Json.JsonSerializer.Deserialize<Dot9Settings>(json)!;

        Assert.Equal(HotkeyBinding.DefaultToggle, settings.Hotkeys.ToggleOverlay);
        Assert.Equal(HotkeyBinding.DefaultEmergency, settings.Hotkeys.EmergencyOff);
    }

    [Theory]
    [InlineData(ModifierKeys.Control | ModifierKeys.Alt, Key.D, true)]
    [InlineData(ModifierKeys.Windows, Key.D, true)]
    [InlineData(ModifierKeys.None, Key.F9, true)]
    [InlineData(ModifierKeys.None, Key.F24, true)]
    [InlineData(ModifierKeys.None, Key.Enter, false)]
    [InlineData(ModifierKeys.None, Key.Space, false)]
    [InlineData(ModifierKeys.None, Key.A, false)]
    [InlineData(ModifierKeys.Shift, Key.A, false)]
    public void IsSafeToHoldAlways_RequiresModifierOrFunctionKey(ModifierKeys mods, Key key, bool expected)
    {
        var binding = new HotkeyBinding { Modifiers = mods, Key = key };
        Assert.Equal(expected, binding.IsSafeToHoldAlways);
    }
}
