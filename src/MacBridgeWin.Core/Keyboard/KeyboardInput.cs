namespace MacBridgeWin.Core.Keyboard;

public sealed record KeyboardInput(
    string Key,
    ShortcutModifiers Modifiers,
    bool IsKeyDown,
    bool IsSynthetic);
