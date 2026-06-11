using System.IO;
using Dot9.Models;
using Dot9.Services;
using Xunit;

namespace Dot9.Tests;

public class SettingsStoreTests : IDisposable
{
    private readonly string _dir;

    public SettingsStoreTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "Dot9Tests_" + Guid.NewGuid().ToString("N"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, recursive: true);
        }
    }

    [Fact]
    public void Load_MissingFile_ReturnsDefault()
    {
        var store = new SettingsStore(_dir);

        var settings = store.Load();

        Assert.Equal("Gentle", settings.ActivePreset);
    }

    [Fact]
    public void Load_CorruptJson_ReturnsDefault()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path.Combine(_dir, "settings.json"), "{ this is not valid json ]");

        var store = new SettingsStore(_dir);
        var settings = store.Load();

        Assert.Equal("Gentle", settings.ActivePreset);
    }

    [Fact]
    public void Save_Then_Load_RoundTrips()
    {
        var store = new SettingsStore(_dir);
        var original = Dot9Settings.CreateDefault();
        original.MonitorId = "Primary";
        original.Dots.DotsPerEdge = 11;

        store.Save(original);
        var restored = store.Load();

        Assert.Equal("Primary", restored.MonitorId);
        Assert.Equal(11, restored.Dots.DotsPerEdge);
    }

    [Fact]
    public void Save_OverExistingFile_ReplacesAndLeavesNoTempFile()
    {
        var store = new SettingsStore(_dir);
        var first = Dot9Settings.CreateDefault();
        first.Dots.DotsPerEdge = 5;
        var second = Dot9Settings.CreateDefault();
        second.Dots.DotsPerEdge = 7;

        store.Save(first);
        store.Save(second);

        Assert.Equal(7, store.Load().Dots.DotsPerEdge);
        Assert.False(File.Exists(Path.Combine(_dir, "settings.json.tmp")));
    }

    [Fact]
    public void SaveBackup_WritesSeparateRecoverableFile()
    {
        var store = new SettingsStore(_dir);
        var live = Dot9Settings.CreateDefault();
        live.Dots.DotsPerEdge = 9;
        var outgoing = Dot9Settings.CreateDefault();
        outgoing.ActivePreset = "Custom";
        outgoing.Dots.DotsPerEdge = 21;

        store.Save(live);
        store.SaveBackup(outgoing);

        // The live settings file is untouched and the snapshot sits alongside it.
        Assert.Equal(9, store.Load().Dots.DotsPerEdge);
        var backupJson = File.ReadAllText(Path.Combine(_dir, "settings.backup.json"));
        Assert.Contains("\"Custom\"", backupJson);
        Assert.Contains("21", backupJson);
    }
}
