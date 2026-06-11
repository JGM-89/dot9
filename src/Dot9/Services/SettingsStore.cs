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
            WriteAtomic(SettingsPath, JsonSerializer.Serialize(settings, _options));
        }
        catch (Exception ex)
        {
            // Settings persistence should never interrupt the overlay during play.
            Log.Warn("Failed to save settings.", ex);
        }
    }

    /// <summary>
    /// Snapshots the outgoing settings to settings.backup.json before a preset
    /// wholesale-replaces them, so a mis-click on "Use preset" is recoverable.
    /// </summary>
    public void SaveBackup(Dot9Settings settings)
    {
        try
        {
            WriteAtomic(BackupPath, JsonSerializer.Serialize(settings, _options));
        }
        catch (Exception ex)
        {
            Log.Warn("Failed to save settings backup.", ex);
        }
    }

    private void WriteAtomic(string path, string contents)
    {
        // Write-then-replace so a crash mid-write can't corrupt the live file
        // (a corrupt settings.json silently resets the user's tuning to defaults).
        Directory.CreateDirectory(_appDirectory);
        var temp = path + ".tmp";
        File.WriteAllText(temp, contents);
        if (File.Exists(path))
        {
            File.Replace(temp, path, destinationBackupFileName: null);
        }
        else
        {
            File.Move(temp, path);
        }
    }

    private static string DefaultAppDirectory => AppPaths.AppDirectory;

    private string SettingsPath => Path.Combine(_appDirectory, "settings.json");

    private string BackupPath => Path.Combine(_appDirectory, "settings.backup.json");
}
