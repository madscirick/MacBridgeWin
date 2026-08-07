namespace MacBridgeWin.Core.Keyboard;

[Flags]
public enum ShortcutModifiers
{
    None = 0,
    Shift = 1,
    Ctrl = 2,
    Alt = 4,
    Win = 8
}
