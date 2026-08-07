using Microsoft.Win32;

namespace MacBridgeWin.App.Services.Startup;

public sealed class WindowsStartupService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "MacBridgeWin";

    public void Apply(bool startWithWindows)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
            ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);

        if (startWithWindows)
        {
            key.SetValue(ValueName, $"\"{Environment.ProcessPath}\"", RegistryValueKind.String);
            return;
        }

        if (key.GetValue(ValueName) is not null)
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
        }
    }
}
