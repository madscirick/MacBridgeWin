using MacBridgeWin.Core.Configuration;

namespace MacBridgeWin.Core.Profiles;

public sealed record ResolvedProfileConfiguration(
    string? ProfileName,
    KeyboardConfiguration Keyboard,
    MouseGestureConfiguration MouseGestures);
