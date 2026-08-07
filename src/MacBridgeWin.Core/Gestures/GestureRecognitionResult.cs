namespace MacBridgeWin.Core.Gestures;

public sealed record GestureRecognitionResult(bool IsRecognized, GestureDirection? Direction, IReadOnlyList<GestureDirection> Sequence)
{
    public static GestureRecognitionResult None { get; } = new(false, null, []);

    public static GestureRecognitionResult Recognized(GestureDirection direction)
    {
        return new GestureRecognitionResult(true, direction, [direction]);
    }

    public static GestureRecognitionResult Recognized(IReadOnlyList<GestureDirection> sequence)
    {
        return sequence.Count == 0
            ? None
            : new GestureRecognitionResult(true, sequence[0], sequence);
    }
}
