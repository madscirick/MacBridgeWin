using MacBridgeWin.Core.Gestures;
using System.Windows;
using System.Windows.Media;
using MediaBrush = System.Windows.Media.Brush;
using MediaColor = System.Windows.Media.Color;
using MediaPen = System.Windows.Media.Pen;
using WpfPoint = System.Windows.Point;
using WpfSize = System.Windows.Size;

namespace MacBridgeWin.App.Controls;

public sealed class GesturePreviewControl : FrameworkElement
{
    public static readonly DependencyProperty GestureProperty = DependencyProperty.Register(
        nameof(Gesture),
        typeof(string),
        typeof(GesturePreviewControl),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

    private static readonly MediaPen TrailPen = new(
        new SolidColorBrush(MediaColor.FromRgb(70, 170, 255)),
        3)
    {
        StartLineCap = PenLineCap.Round,
        EndLineCap = PenLineCap.Round,
        LineJoin = PenLineJoin.Round
    };

    private static readonly MediaBrush StartBrush = new SolidColorBrush(MediaColor.FromRgb(255, 96, 116));
    private const string CrossGesture = "DownLeft,Up,DownRight";

    public string Gesture
    {
        get => (string)GetValue(GestureProperty);
        set => SetValue(GestureProperty, value);
    }

    protected override WpfSize MeasureOverride(WpfSize availableSize)
    {
        return new WpfSize(72, 36);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (!GestureSequence.TryParse(Gesture, out var sequence) || sequence.Count == 0)
        {
            return;
        }

        if (string.Equals(Gesture, CrossGesture, StringComparison.OrdinalIgnoreCase))
        {
            DrawCross(drawingContext);
            return;
        }

        var points = BuildPoints(sequence);
        if (points.Count == 0)
        {
            return;
        }

        drawingContext.DrawEllipse(StartBrush, null, points[0], 3, 3);

        if (points.Count < 2)
        {
            return;
        }

        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(points[0], isFilled: false, isClosed: false);
            context.PolyLineTo(points.Skip(1).ToArray(), isStroked: true, isSmoothJoin: true);
        }

        geometry.Freeze();
        drawingContext.DrawGeometry(null, TrailPen, geometry);
    }

    private void DrawCross(DrawingContext drawingContext)
    {
        const double halfSize = 9;
        var left = Math.Max(4, (ActualWidth / 2) - halfSize);
        var right = Math.Min(ActualWidth - 4, (ActualWidth / 2) + halfSize);
        var top = Math.Max(4, (ActualHeight / 2) - halfSize);
        var bottom = Math.Min(ActualHeight - 4, (ActualHeight / 2) + halfSize);

        var topRight = new WpfPoint(right, top);
        var bottomLeft = new WpfPoint(left, bottom);
        var bottomRight = new WpfPoint(right, bottom);

        var path = new StreamGeometry();
        using (var context = path.Open())
        {
            context.BeginFigure(topRight, isFilled: false, isClosed: false);
            context.PolyLineTo([bottomLeft, new WpfPoint(left, top), bottomRight], isStroked: true, isSmoothJoin: true);
        }

        path.Freeze();
        drawingContext.DrawGeometry(null, TrailPen, path);
        drawingContext.DrawEllipse(StartBrush, null, topRight, 3, 3);
    }

    private List<WpfPoint> BuildPoints(IReadOnlyList<GestureDirection> sequence)
    {
        const double step = 18;
        var points = new List<WpfPoint> { new(ActualWidth / 2, ActualHeight / 2) };

        foreach (var direction in sequence)
        {
            var previous = points[^1];
            var offset = GetOffset(direction, step);
            points.Add(new WpfPoint(previous.X + offset.X, previous.Y + offset.Y));
        }

        var minX = points.Min(point => point.X);
        var maxX = points.Max(point => point.X);
        var minY = points.Min(point => point.Y);
        var maxY = points.Max(point => point.Y);
        var width = Math.Max(1, maxX - minX);
        var height = Math.Max(1, maxY - minY);
        var scale = Math.Min((ActualWidth - 10) / width, (ActualHeight - 10) / height);
        scale = Math.Min(1, scale);
        var offsetX = ((ActualWidth - (width * scale)) / 2) - (minX * scale);
        var offsetY = ((ActualHeight - (height * scale)) / 2) - (minY * scale);

        return points
            .Select(point => new WpfPoint((point.X * scale) + offsetX, (point.Y * scale) + offsetY))
            .ToList();
    }

    private static Vector GetOffset(GestureDirection direction, double step)
    {
        return direction switch
        {
            GestureDirection.Left => new Vector(-step, 0),
            GestureDirection.Right => new Vector(step, 0),
            GestureDirection.Up => new Vector(0, -step),
            GestureDirection.Down => new Vector(0, step),
            GestureDirection.UpLeft => new Vector(-step, -step),
            GestureDirection.UpRight => new Vector(step, -step),
            GestureDirection.DownLeft => new Vector(-step, step),
            GestureDirection.DownRight => new Vector(step, step),
            _ => new Vector()
        };
    }
}
