using MacBridgeWin.Core.Configuration;

namespace MacBridgeWin.App.ViewModels;

public sealed class KeyboardMappingViewModel
{
    public string SourceShortcut { get; set; } = string.Empty;

    public string TargetShortcut { get; set; } = string.Empty;

    public static KeyboardMappingViewModel FromConfiguration(KeyboardMappingConfiguration mapping)
    {
        return new KeyboardMappingViewModel
        {
            SourceShortcut = mapping.SourceShortcut,
            TargetShortcut = mapping.TargetShortcut
        };
    }

    public KeyboardMappingConfiguration ToConfiguration()
    {
        return new KeyboardMappingConfiguration(SourceShortcut, TargetShortcut);
    }
}
