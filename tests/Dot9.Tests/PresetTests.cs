using Dot9.Models;
using Xunit;

namespace Dot9.Tests;

public class PresetTests
{
    public static IEnumerable<object[]> AllPresets()
    {
        foreach (var preset in Presets.All)
        {
            yield return new object[] { preset };
        }
    }

    [Theory]
    [MemberData(nameof(AllPresets))]
    public void CreateSettings_ActivePresetMatchesDefinition(PresetDefinition preset)
    {
        var settings = preset.CreateSettings();
        Assert.Equal(preset.ShortName, settings.ActivePreset);
    }

    [Fact]
    public void CreateSettings_ReturnsIndependentInstances()
    {
        var first = Presets.Gentle.CreateSettings();
        var second = Presets.Gentle.CreateSettings();

        first.Dots.Opacity = 0.99;

        Assert.NotSame(first, second);
        Assert.NotEqual(first.Dots.Opacity, second.Dots.Opacity);
    }

    [Fact]
    public void All_ContainsTheFourPresets()
    {
        Assert.Equal(4, Presets.All.Count);
        Assert.Contains(Presets.Gentle, Presets.All);
        Assert.Contains(Presets.Fps, Presets.All);
        Assert.Contains(Presets.Vertigo, Presets.All);
        Assert.Contains(Presets.FastMotion, Presets.All);
    }
}
