namespace MacBridgeWin.Core.Configuration;

public sealed class MouseGestureConfiguration
{
    public int MovementThresholdPixels { get; set; } = 48;

    public List<GestureActionConfiguration> Gestures { get; set; } = [];
}
