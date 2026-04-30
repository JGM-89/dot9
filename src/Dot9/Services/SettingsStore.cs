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
        catch
        {
            return Dot9Settings.CreateDefault();
        }
    }

    public void Save(Dot9Settings settings)
    {
        try
        {
            Directory.CreateDirectory(AppDirectory);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, _options));
        }
        catch
        {
            // Settings persistence should never interrupt the overlay during play.
        }
    }

    private static string AppDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dot9");

    private static string SettingsPath => Path.Combine(AppDirectory, "settings.json");
}
