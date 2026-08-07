namespace MacBridgeWin.Core.Keyboard;

public sealed record KeyboardShortcut(ShortcutModifiers Modifiers, string Key)
{
    public static bool TryParse(string value, out KeyboardShortcut shortcut)
    {
        shortcut = new KeyboardShortcut(ShortcutModifiers.None, string.Empty);

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var modifiers = ShortcutModifiers.None;
        var key = string.Empty;
        var parts = value.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            if (TryParseModifier(part, out var modifier))
            {
                modifiers |= modifier;
                continue;
            }

            if (key.Length > 0)
            {
                return false;
            }

            key = NormalizeKey(part);
        }

        if (key.Length == 0)
        {
            return false;
        }

        shortcut = new KeyboardShortcut(modifiers, key);
        return true;
    }

    public static KeyboardShortcut Parse(string value)
    {
        if (!TryParse(value, out var shortcut))
        {
            throw new FormatException($"'{value}' is not a valid keyboard shortcut.");
        }

        return shortcut;
    }

    public override string ToString()
    {
        var parts = new List<string>();

        if (Modifiers.HasFlag(ShortcutModifiers.Ctrl))
        {
            parts.Add("Ctrl");
        }

        if (Modifiers.HasFlag(ShortcutModifiers.Alt))
        {
            parts.Add("Alt");
        }

        if (Modifiers.HasFlag(ShortcutModifiers.Shift))
        {
            parts.Add("Shift");
        }

        if (Modifiers.HasFlag(ShortcutModifiers.Win))
        {
            parts.Add("Win");
        }

        parts.Add(Key);
        return string.Join("+", parts);
    }

    private static bool TryParseModifier(string value, out ShortcutModifiers modifier)
    {
        modifier = value.Trim().ToUpperInvariant() switch
        {
            "CTRL" or "CONTROL" => ShortcutModifiers.Ctrl,
            "ALT" => ShortcutModifiers.Alt,
            "SHIFT" => ShortcutModifiers.Shift,
            "WIN" or "WINDOWS" => ShortcutModifiers.Win,
            _ => ShortcutModifiers.None
        };

        return modifier != ShortcutModifiers.None;
    }

    private static string NormalizeKey(string value)
    {
        return value.Trim().ToUpperInvariant();
    }
}
