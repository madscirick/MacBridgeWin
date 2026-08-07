using System.IO;

namespace MacBridgeWin.App;

public static class AppPaths
{
    public static string GetConfigurationPath()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MacBridgeWin");

        return Path.Combine(directory, "config.json");
    }
}
