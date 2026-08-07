namespace MacBridgeWin.Core.Configuration;

public sealed class ApplicationProfileConfiguration
{
    public string Name { get; set; } = string.Empty;

    public string ProcessName { get; set; } = string.Empty;

    public KeyboardConfiguration Keyboard { get; set; } = new();

    public MouseGestureConfiguration MouseGestures { get; set; } = new();
}
