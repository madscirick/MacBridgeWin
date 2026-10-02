using MacBridgeWin.Core.Configuration;

namespace MacBridgeWin.Core.Gestures;

public static class GestureBindingResolver
{
    public static GestureActionConfiguration? Resolve(MouseGestureConfiguration configuration, IReadOnlyList<GestureDirection> sequence)
    {
        if (sequence.Count == 0) return null;
        var candidates = configuration.Gestures.Where(binding => binding.Enabled)
            .Where(binding => GestureSequence.TryParse(binding.Gesture, out _));
        var binding = candidates.FirstOrDefault(binding => Matches(binding, sequence, false))
            ?? candidates.FirstOrDefault(binding => Matches(binding, sequence, true));
        return binding is null || string.IsNullOrWhiteSpace(binding.Action)
            || string.Equals(binding.Action, "None", StringComparison.OrdinalIgnoreCase) ? null : binding;
    }

    private static bool Matches(GestureActionConfiguration binding, IReadOnlyList<GestureDirection> sequence, bool compatible)
    {
        GestureSequence.TryParse(binding.Gesture, out var configured);
        return compatible ? GestureSequence.IsCompatible(configured, sequence) : GestureSequence.Equals(configured, sequence);
    }
}
