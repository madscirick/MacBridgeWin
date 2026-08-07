using System.Drawing;
using System.Windows.Forms;

namespace MacBridgeWin.App.Services;

public sealed class TrayIconService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;

    public TrayIconService(Action showSettings, Action exitApplication)
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Settings", null, (_, _) => showSettings());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => exitApplication());

        _notifyIcon = new NotifyIcon
        {
            Text = "MacBridgeWin",
            Icon = Icon.ExtractAssociatedIcon(Environment.ProcessPath ?? string.Empty) ?? SystemIcons.Application,
            ContextMenuStrip = menu,
            Visible = false
        };

        _notifyIcon.DoubleClick += (_, _) => showSettings();
    }

    public void Show()
    {
        _notifyIcon.Visible = true;
    }

    public void ShowStartupNotification()
    {
        _notifyIcon.ShowBalloonTip(
            3000,
            "MacBridgeWin",
            "MacBridgeWin is running. Open Settings from the tray icon.",
            ToolTipIcon.Info);
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.ContextMenuStrip?.Dispose();
        _notifyIcon.Dispose();
    }
}
