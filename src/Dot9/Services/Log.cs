using System.IO;

namespace Dot9.Services;

/// <summary>
/// Minimal, dependency-free diagnostics. Writes to the Debug output and to a
/// small rolling log file under %APPDATA%\Dot9\logs so user-reported issues
/// (failed hotkey registration, DPI detection, settings load, etc.) can be
/// diagnosed. Logging must never throw or interrupt the overlay.
/// </summary>
public static class Log
{
    private const long MaxBytes = 1024 * 1024; // rotate at ~1 MB
    private static readonly object Gate = new();

    private static string LogDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dot9", "logs");

    private static string LogPath => Path.Combine(LogDirectory, "dot9.log");

    public static void Info(string message) => Write("INFO", message, null);
    public static void Warn(string message, Exception? ex = null) => Write("WARN", message, ex);
    public static void Error(string message, Exception? ex = null) => Write("ERROR", message, ex);

    private static void Write(string level, string message, Exception? ex)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
        if (ex is not null)
        {
            line += $" | {ex.GetType().Name}: {ex.Message}";
        }

        System.Diagnostics.Debug.WriteLine(line);

        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(LogDirectory);
                Rotate();
                File.AppendAllText(LogPath, line + Environment.NewLine);
            }
        }
        catch
        {
            // Logging is best-effort; never let it disrupt the app.
        }
    }

    private static void Rotate()
    {
        try
        {
            var info = new FileInfo(LogPath);
            if (info.Exists && info.Length > MaxBytes)
            {
                var old = LogPath + ".old";
                File.Delete(old);
                File.Move(LogPath, old);
            }
        }
        catch
        {
            // If rotation fails, just keep appending.
        }
    }
}
