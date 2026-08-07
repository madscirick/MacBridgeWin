using System.Runtime.InteropServices;

namespace MacBridgeWin.App.Services.Mouse;

public sealed class BrowserCommandInputSender
{
    private const int WmAppCommand = 0x0319;
    private const int AppCommandBrowserBack = 1;
    private const int AppCommandBrowserForward = 2;
    private const int AppCommandBrowserRefresh = 3;

    public bool TrySend(string action)
    {
        var command = action switch
        {
            Core.Configuration.GestureActionCatalog.BrowserBack => AppCommandBrowserBack,
            Core.Configuration.GestureActionCatalog.BrowserForward => AppCommandBrowserForward,
            Core.Configuration.GestureActionCatalog.Refresh => AppCommandBrowserRefresh,
            _ => 0
        };

        if (command == 0)
        {
            return false;
        }

        var foregroundWindow = GetForegroundWindow();
        if (foregroundWindow == IntPtr.Zero)
        {
            return false;
        }

        SendMessage(foregroundWindow, WmAppCommand, foregroundWindow, (IntPtr)(command << 16));
        return true;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
}
