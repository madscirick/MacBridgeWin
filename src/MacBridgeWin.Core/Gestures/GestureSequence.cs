namespace MacBridgeWin.Core.Gestures;

public static class GestureSequence
{
    private static readonly GestureDirection[] CrossXSequence =
    [
        GestureDirection.DownLeft,
        GestureDirection.Up,
        GestureDirection.DownRight
    ];

    private static readonly GestureDirection[] VDownSequence =
    [
        GestureDirection.DownLeft,
        GestureDirection.DownRight
    ];

    public static bool TryParse(string value, out IReadOnlyList<GestureDirection> sequence)
    {
        sequence = [];

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var parts = value.Contains(',')
            ? value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : SplitCompactSequence(value);

        var directions = new List<GestureDirection>();
        foreach (var part in parts)
        {
            if (!Enum.TryParse<GestureDirection>(part, ignoreCase: true, out var direction))
            {
                return false;
            }

            directions.Add(direction);
        }

        sequence = directions;
        return directions.Count > 0;
    }

    public static bool Equals(IReadOnlyList<GestureDirection> left, IReadOnlyList<GestureDirection> right)
    {
        return left.SequenceEqual(right);
    }

    public static bool IsCompatible(IReadOnlyList<GestureDirection> configured, IReadOnlyList<GestureDirection> recognized)
    {
        if (Equals(configured, recognized))
        {
            return true;
        }

        var expandedRecognized = ExpandDiagonals(recognized);
        if (configured.SequenceEqual(expandedRecognized))
        {
            return true;
        }

        if (configured.SequenceEqual(CrossXSequence))
        {
            return IsOrderedSubsequence(ExpandDiagonals(configured), expandedRecognized)
                || IsContinuousCrossX(recognized);
        }

        return configured.SequenceEqual(VDownSequence) && IsVDown(recognized);
    }

    private static bool IsVDown(IReadOnlyList<GestureDirection> sequence)
    {
        // Accept a geometric V drawn left-to-right or right-to-left. The sampled
        // recognizer can also produce horizontal lead-in/out segments.
        var compact = sequence.Where(direction => direction is not GestureDirection.Left and not GestureDirection.Right).ToList();
        if (compact.SequenceEqual(new[] { GestureDirection.DownRight, GestureDirection.UpRight })
            || compact.SequenceEqual(new[] { GestureDirection.DownLeft, GestureDirection.UpLeft }))
        {
            return true;
        }

        if (sequence.Any(IsUpward))
        {
            return false;
        }

        var downLeftIndex = FindNext(sequence, 0, direction => direction == GestureDirection.DownLeft);
        if (downLeftIndex >= 0
            && FindNext(sequence, downLeftIndex + 1, direction =>
                direction is GestureDirection.DownRight or GestureDirection.Right) >= 0)
        {
            return true;
        }

        var leftIndex = FindNext(sequence, 0, direction => direction == GestureDirection.Left);
        if (leftIndex >= 0
            && FindNext(sequence, leftIndex + 1, direction => direction == GestureDirection.DownRight) >= 0)
        {
            return true;
        }

        return sequence.Count == 3
            && sequence[0] == GestureDirection.Left
            && sequence[1] == GestureDirection.DownRight
            && sequence[2] == GestureDirection.Right;
    }

    private static bool IsOrderedSubsequence(
        IReadOnlyList<GestureDirection> expected,
        IReadOnlyList<GestureDirection> actual)
    {
        var expectedIndex = 0;
        foreach (var direction in actual)
        {
            if (expectedIndex < expected.Count && direction == expected[expectedIndex])
            {
                expectedIndex++;
            }
        }

        return expectedIndex == expected.Count;
    }

    private static bool IsContinuousCrossX(IReadOnlyList<GestureDirection> sequence)
    {
        var upwardIndex = -1;
        for (var index = 0; index < sequence.Count; index++)
        {
            if (IsDownward(sequence[index]))
            {
                upwardIndex = FindNext(sequence, index + 1, IsUpward);
                break;
            }
        }

        if (upwardIndex < 0)
        {
            return false;
        }

        var hasDownwardFinalSegment = false;
        var hasRightwardFinalSegment = false;
        for (var index = upwardIndex + 1; index < sequence.Count; index++)
        {
            hasDownwardFinalSegment |= IsDownward(sequence[index]);
            hasRightwardFinalSegment |= IsRightward(sequence[index]);
        }

        return hasDownwardFinalSegment && hasRightwardFinalSegment;
    }

    private static int FindNext(
        IReadOnlyList<GestureDirection> sequence,
        int startIndex,
        Func<GestureDirection, bool> predicate)
    {
        for (var index = startIndex; index < sequence.Count; index++)
        {
            if (predicate(sequence[index]))
            {
                return index;
            }
        }

        return -1;
    }

    private static bool IsDownward(GestureDirection direction) =>
        direction is GestureDirection.Down or GestureDirection.DownLeft or GestureDirection.DownRight;

    private static bool IsUpward(GestureDirection direction) =>
        direction is GestureDirection.Up or GestureDirection.UpLeft or GestureDirection.UpRight;

    private static bool IsRightward(GestureDirection direction) =>
        direction is GestureDirection.Right or GestureDirection.UpRight or GestureDirection.DownRight;

    private static IReadOnlyList<GestureDirection> ExpandDiagonals(IReadOnlyList<GestureDirection> sequence)
    {
        var expanded = new List<GestureDirection>();
        foreach (var direction in sequence)
        {
            foreach (var expandedDirection in ExpandDirection(direction))
            {
                if (expanded.Count == 0 || expanded[^1] != expandedDirection)
                {
                    expanded.Add(expandedDirection);
                }
            }
        }

        return expanded;
    }

    private static IReadOnlyList<GestureDirection> ExpandDirection(GestureDirection direction)
    {
        return direction switch
        {
            GestureDirection.UpLeft => [GestureDirection.Up, GestureDirection.Left],
            GestureDirection.UpRight => [GestureDirection.Up, GestureDirection.Right],
            GestureDirection.DownLeft => [GestureDirection.Down, GestureDirection.Left],
            GestureDirection.DownRight => [GestureDirection.Down, GestureDirection.Right],
            _ => [direction]
        };
    }

    private static IReadOnlyList<string> SplitCompactSequence(string value)
    {
        var remaining = value.Trim();
        var parts = new List<string>();

        while (remaining.Length > 0)
        {
            var match = Enum.GetNames<GestureDirection>()
                .OrderByDescending(name => name.Length)
                .FirstOrDefault(name => remaining.StartsWith(name, StringComparison.OrdinalIgnoreCase));
            if (match is null)
            {
                return [remaining];
            }

            parts.Add(match);
            remaining = remaining[match.Length..];
        }

        return parts;
    }
}
