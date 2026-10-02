namespace MacBridgeWin.Core.Gestures;

public sealed class BasicDirectionalGestureRecognizer
{
    private const double DominanceRatio = 1.35;

    public static bool HasThresholdMovement(IReadOnlyList<GesturePoint> points, int thresholdPixels)
    {
        if (points.Count < 2 || thresholdPixels <= 0) return false;
        var start = points[0];
        for (var index = 1; index < points.Count; index++)
        {
            if (Math.Abs((long)points[index].X - start.X) >= thresholdPixels
                || Math.Abs((long)points[index].Y - start.Y) >= thresholdPixels) return true;
        }
        return false;
    }

    public GestureRecognitionResult Recognize(IReadOnlyList<GesturePoint> points, int thresholdPixels)
    {
        var sequence = RecognizeSequence(points, thresholdPixels);
        return sequence.Count == 1
            ? GestureRecognitionResult.Recognized(sequence)
            : GestureRecognitionResult.None;
    }

    public IReadOnlyList<GestureDirection> RecognizeSequence(IReadOnlyList<GesturePoint> points, int thresholdPixels)
    {
        if (points.Count < 2 || thresholdPixels <= 0)
        {
            return [];
        }

        var directions = new List<GestureDirection>();
        var anchor = points[0];

        for (var index = 1; index < points.Count; index++)
        {
            var current = points[index];
            var direction = RecognizeSegment(anchor, current, thresholdPixels);
            if (direction is null)
            {
                continue;
            }

            if (directions.Count == 0 || directions[^1] != direction.Value)
            {
                directions.Add(direction.Value);
            }

            anchor = current;
        }

        return directions;
    }

    private static GestureDirection? RecognizeSegment(GesturePoint start, GesturePoint end, int thresholdPixels)
    {
        var deltaX = end.X - start.X;
        var deltaY = end.Y - start.Y;
        var absoluteX = Math.Abs(deltaX);
        var absoluteY = Math.Abs(deltaY);

        if (absoluteX < thresholdPixels && absoluteY < thresholdPixels)
        {
            return null;
        }

        if (absoluteX >= absoluteY * DominanceRatio)
        {
            return deltaX < 0 ? GestureDirection.Left : GestureDirection.Right;
        }

        if (absoluteY >= absoluteX * DominanceRatio)
        {
            return deltaY < 0 ? GestureDirection.Up : GestureDirection.Down;
        }

        return (deltaX, deltaY) switch
        {
            (< 0, < 0) => GestureDirection.UpLeft,
            (> 0, < 0) => GestureDirection.UpRight,
            (< 0, > 0) => GestureDirection.DownLeft,
            (> 0, > 0) => GestureDirection.DownRight,
            _ => null
        };
    }
}
