namespace MacBridgeWin.Core.Configuration;

public sealed record GestureActionConfiguration(
    string Gesture,
    string Action,
    bool Enabled = true,
    string ModifierKey = GestureCatalog.RightButtonModifier,
    bool ShowCursor = false,
    string Note = "");
