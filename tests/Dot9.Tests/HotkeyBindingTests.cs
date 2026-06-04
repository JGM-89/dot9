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
}
