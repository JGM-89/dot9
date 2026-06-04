using System.IO;
using System.Text.Json;
using Dot9.Models;

namespace Dot9.Services;

public sealed class SettingsStore
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    private readonly string _appDirectory;

    public SettingsStore(string? directory = null)
    {
        _appDirectory = directory ?? DefaultAppDirectory;
    }

    public Dot9Settings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                return Dot9Settings.CreateDefault();
            }

            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<Dot9Settings>(json, _options) ?? Dot9Settings.CreateDefault();
        }
        catch (Exception ex)
        {
            Log.Warn("Failed to load settings; falling back to defaults.", ex);
            return Dot9Settings.CreateDefault();
        }
    }

    public void Save(Dot9Settings settings)
    {
        try
        {
            Directory.CreateDirectory(_appDirectory);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, _options));
        }
        catch (Exception ex)
        {
            // Settings persistence should never interrupt the overlay during play.
            Log.Warn("Failed to save settings.", ex);
        }
    }

    private static string DefaultAppDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dot9");

    private string SettingsPath => Path.Combine(_appDirectory, "settings.json");
}
