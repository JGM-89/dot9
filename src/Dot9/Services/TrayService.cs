using Dot9.Models;
using System.IO;
using Forms = System.Windows.Forms;

namespace Dot9.Services;

public sealed class TrayService : IDisposable
{
    private readonly AppState _state;
    private readonly Action _showSettings;
    private readonly Action _quit;
    private readonly Forms.NotifyIcon _notifyIcon;
    private bool _hasShownMinimizeNotice;

    public TrayService(AppState state, Action showSettings, Action quit)
    {
        _state = state;
        _showSettings = showSettings;
        _quit = quit;

        _notifyIcon = new Forms.NotifyIcon
        {
            Text = "Dot[9]",
            Icon = LoadTrayIcon(),
            Visible = true,
            ContextMenuStrip = BuildMenu()
        };

        _notifyIcon.DoubleClick += (_, _) => _showSettings();
        _state.OverlayEnabledChanged += (_, _) => Refresh();
    }

    public void ShowMinimizedToTrayNotice()
    {
        if (_hasShownMinimizeNotice)
        {
            return;
        }

        _hasShownMinimizeNotice = true;
        _notifyIcon.ShowBalloonTip(
            4500,
            "Dot[9] is still running",
            "Dot[9] was minimized to the system tray. Double-click the tray icon to open settings again.",
            Forms.ToolTipIcon.Info);
    }

    private Forms.ContextMenuStrip BuildMenu()
    {
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Dot[9] status: Off").Enabled = false;
        menu.Items.Add("Toggle overlay", null, (_, _) => _state.ToggleOverlay());
        menu.Items.Add("Open settings", null, (_, _) => _showSettings());
        menu.Items.Add("Emergency off", null, (_, _) => _state.EmergencyOff());
        menu.Items.Add(new Forms.ToolStripSeparator());
        foreach (var preset in Presets.All)
        {
            menu.Items.Add($"Use preset: {preset.ShortName}", null, (_, _) => _state.ApplyPreset(preset));
        }
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Quit", null, (_, _) => _quit());
        return menu;
    }

    private void Refresh()
    {
        if (_notifyIcon.ContextMenuStrip?.Items.Count > 0)
        {
            _notifyIcon.ContextMenuStrip.Items[0].Text = $"Dot[9] status: {_state.OverlayStatusText}";
        }
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }

    private static System.Drawing.Icon LoadTrayIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "dot9.ico");
        return File.Exists(iconPath)
            ? new System.Drawing.Icon(iconPath)
            : System.Drawing.SystemIcons.Application;
    }
}
