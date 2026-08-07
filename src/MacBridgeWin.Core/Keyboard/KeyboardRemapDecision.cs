namespace MacBridgeWin.Core.Keyboard;

public sealed record KeyboardRemapDecision(bool ShouldSuppressOriginal, KeyboardShortcut? ReplacementShortcut)
{
    public static KeyboardRemapDecision PassThrough { get; } = new(false, null);

    public static KeyboardRemapDecision RemapTo(KeyboardShortcut replacementShortcut)
    {
        return new KeyboardRemapDecision(true, replacementShortcut);
    }
}
