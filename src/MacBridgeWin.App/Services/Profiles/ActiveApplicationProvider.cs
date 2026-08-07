using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MacBridgeWin.App.Services.Profiles;

public sealed class ActiveApplicationProvider
{
    public string? GetActiveProcessName()
    {
        var foregroundWindow = GetForegroundWindow();
        return GetProcessNameFromWindow(foregroundWindow);
    }

    public string? GetProcessNameFromPoint(int x, int y)
    {
        var window = WindowFromPoint(new Point(x, y));
        if (window == IntPtr.Zero)
        {
            return null;
        }

        var rootWindow = GetAncestor(window, GetAncestorRoot);
        return GetProcessNameFromWindow(rootWindow == IntPtr.Zero ? window : rootWindow);
    }

    private static string? GetProcessNameFromWindow(IntPtr window)
    {
        var foregroundWindow = window;
        if (foregroundWindow == IntPtr.Zero)
        {
            return null;
        }

        GetWindowThreadProcessId(foregroundWindow, out var processId);
        if (processId == 0)
        {
            return null;
        }

        try
        {
            using var process = Process.GetProcessById((int)processId);
            return process.ProcessName;
        }
        catch
        {
            return null;
        }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(Point point);

    [DllImport("user32.dll")]
    private static extern IntPtr GetAncestor(IntPtr hwnd, uint flags);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    private const uint GetAncestorRoot = 2;

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Point(int x, int y)
    {
        public readonly int X = x;
        public readonly int Y = y;
    }
}
