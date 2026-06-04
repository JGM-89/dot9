using System.IO;

namespace Dot9.Services;

/// <summary>
/// Resolves the app's per-user data directory. Debug builds use a separate folder
/// so development/testing never reads or writes the installed app's real settings
/// and logs (which previously caused dev runs to mark onboarding as already seen).
/// </summary>
public static class AppPaths
{
    public static string AppDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
#if DEBUG
        "Dot9-Dev");
#else
        "Dot9");
#endif
}
