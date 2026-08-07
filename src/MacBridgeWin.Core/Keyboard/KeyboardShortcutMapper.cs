using MacBridgeWin.Core.Configuration;

namespace MacBridgeWin.Core.Keyboard;

public sealed class KeyboardShortcutMapper
{
    private readonly Dictionary<KeyboardShortcut, KeyboardShortcut> _mappings;

    public KeyboardShortcutMapper(IEnumerable<KeyboardMappingConfiguration> mappings)
    {
        _mappings = mappings
            .Select(mapping => new
            {
                SourceParsed = KeyboardShortcut.TryParse(mapping.SourceShortcut, out var source),
                TargetParsed = KeyboardShortcut.TryParse(mapping.TargetShortcut, out var target),
                Source = source,
                Target = target
            })
            .Where(mapping => mapping.SourceParsed && mapping.TargetParsed)
            .ToDictionary(mapping => mapping.Source, mapping => mapping.Target);
    }

    public KeyboardRemapDecision Decide(KeyboardInput input, bool mappingsEnabled)
    {
        if (!mappingsEnabled || !input.IsKeyDown || input.IsSynthetic)
        {
            return KeyboardRemapDecision.PassThrough;
        }

        if (IsProtectedNativeShortcut(input))
        {
            return KeyboardRemapDecision.PassThrough;
        }

        var sourceShortcut = new KeyboardShortcut(input.Modifiers, input.Key.ToUpperInvariant());
        return _mappings.TryGetValue(sourceShortcut, out var targetShortcut)
            ? KeyboardRemapDecision.RemapTo(targetShortcut)
            : KeyboardRemapDecision.PassThrough;
    }

    private static bool IsProtectedNativeShortcut(KeyboardInput input)
    {
        if (!input.Modifiers.HasFlag(ShortcutModifiers.Alt))
        {
            return false;
        }

        return string.Equals(input.Key, "TAB", StringComparison.OrdinalIgnoreCase)
            || string.Equals(input.Key, "F4", StringComparison.OrdinalIgnoreCase);
    }
}
