namespace MacBridgeWin.Core.Configuration;

public static class GestureCatalog
{
    public const string RightButtonModifier = "RightButton";

    public static IReadOnlyList<GestureModifierOption> ModifierOptions { get; } =
    [
        new("Hold Right Button", RightButtonModifier),
        new("Hold Middle Button", "MiddleButton"),
        new("Hold Right Button + Ctrl", "RightButton+Ctrl"),
        new("Hold Right Button + Shift", "RightButton+Shift")
    ];

    public static IReadOnlyList<GestureOption> GestureOptions { get; } =
    [
        new("Left", "Left"),
        new("Right", "Right"),
        new("Up", "Up"),
        new("Down", "Down"),
        new("Up Left", "UpLeft"),
        new("Up Right", "UpRight"),
        new("Down Left", "DownLeft"),
        new("Down Right", "DownRight"),
        new("L: Left then Down", "Left,Down"),
        new("L: Left then Up", "Left,Up"),
        new("L: Right then Down", "Right,Down"),
        new("L: Right then Up", "Right,Up"),
        new("Corner: Down then Right", "Down,Right"),
        new("Corner: Down then Left", "Down,Left"),
        new("Corner: Up then Right", "Up,Right"),
        new("Corner: Up then Left", "Up,Left"),
        new("V Down", "DownLeft,DownRight"),
        new("V Up", "UpLeft,UpRight"),
        new("Slash /", "DownLeft,UpRight"),
        new("Slash \\", "DownRight,UpLeft"),
        new("Cross X", "DownLeft,Up,DownRight"),
        new("Z", "Right,DownLeft,Right")
    ];
}

public sealed record GestureModifierOption(string Name, string Modifier);

public sealed record GestureOption(string Name, string Gesture);
