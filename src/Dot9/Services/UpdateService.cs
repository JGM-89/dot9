using Velopack;
using Velopack.Sources;

namespace Dot9.Services;

public enum UpdateStatus
{
    Idle,
    Checking,
    Downloading,
    ReadyOnRestart,
    UpToDate,
    Failed
}

/// <summary>
/// Wraps Velopack to check GitHub Releases for a newer build, download it in the
/// background, and stage it to apply on the next app restart (never forces a
/// restart mid-session). No-ops when the app is not running from an installed
/// Velopack build (dev/portable), and fails silently when GitHub is unreachable.
/// </summary>
public sealed class UpdateService
{
    private const string RepoUrl = "https://github.com/JGM-89/dot9";

    private UpdateManager? _manager;
    private UpdateInfo? _pendingUpdate;
    private int _checkInProgress;

    public UpdateStatus Status { get; private set; } = UpdateStatus.Idle;
    public string? AvailableVersion { get; private set; }

    public event EventHandler? StatusChanged;
    public event EventHandler<string>? UpdateReady;

    private UpdateManager Manager => _manager ??= new UpdateManager(new GithubSource(RepoUrl, null, false));

    /// <summary>True only when launched from an installed Velopack build (so updates are possible).</summary>
    public bool IsInstalled
    {
        get
        {
            try { return Manager.IsInstalled; }
            catch { return false; }
        }
    }

    public async Task CheckAndStageAsync()
    {
        if (!IsInstalled)
        {
            // Dev build, portable copy, or first run before install — nothing to update.
            return;
        }

        // Spamming "Check for updates" must not start overlapping downloads.
        if (Interlocked.Exchange(ref _checkInProgress, 1) == 1)
        {
            return;
        }

        try
        {
            SetStatus(UpdateStatus.Checking);

            var info = await Manager.CheckForUpdatesAsync();
            if (info is null)
            {
                SetStatus(UpdateStatus.UpToDate);
                return;
            }

            SetStatus(UpdateStatus.Downloading);
            await Manager.DownloadUpdatesAsync(info);

            _pendingUpdate = info;
            AvailableVersion = info.TargetFullRelease.Version.ToString();

            // Apply once this process exits; do not relaunch. The next time the user
            // opens Dot[9] it will be the new version.
            Manager.WaitExitThenApplyUpdates(info, silent: true, restart: false);

            SetStatus(UpdateStatus.ReadyOnRestart);
            Log.Info($"Update {AvailableVersion} downloaded; will apply on next restart.");
            UpdateReady?.Invoke(this, AvailableVersion);
        }
        catch (Exception ex)
        {
            SetStatus(UpdateStatus.Failed);
            Log.Warn("Update check/download failed.", ex);
        }
        finally
        {
            Interlocked.Exchange(ref _checkInProgress, 0);
        }
    }

    /// <summary>Apply a staged update immediately and restart (for an opt-in "Restart now" action).</summary>
    public void RestartToApply()
    {
        if (_pendingUpdate is null)
        {
            return;
        }

        try
        {
            Manager.ApplyUpdatesAndRestart(_pendingUpdate);
        }
        catch (Exception ex)
        {
            Log.Warn("Could not apply update and restart.", ex);
        }
    }

    private void SetStatus(UpdateStatus status)
    {
        Status = status;
        StatusChanged?.Invoke(this, EventArgs.Empty);
    }
}
