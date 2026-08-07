using MacBridgeWin.Core.Gestures;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using MediaBrushes = System.Windows.Media.Brushes;
using MediaColor = System.Windows.Media.Color;
using MediaPen = System.Windows.Media.Pen;
using WpfPoint = System.Windows.Point;

namespace MacBridgeWin.App.Services.Mouse;

public sealed class MouseGestureFeedbackService
{
    private readonly System.Windows.Application _application;
    private readonly ConcurrentQueue<FeedbackEvent> _pendingEvents = new();
    private GestureTrailWindow? _window;
    private int _isDispatchScheduled;
    private bool _isFeedbackDisabled;

    public MouseGestureFeedbackService(System.Windows.Application application)
    {
        _application = application;
    }

    public void Begin(GesturePoint point)
    {
        Enqueue(FeedbackEvent.Begin(point));
    }

    public void AddPoint(GesturePoint point)
    {
        Enqueue(FeedbackEvent.Move(point));
    }

    public void End()
    {
        if (_isFeedbackDisabled)
        {
            return;
        }

        Enqueue(FeedbackEvent.End());
    }

    private void Enqueue(FeedbackEvent feedbackEvent)
    {
        if (_isFeedbackDisabled)
        {
            return;
        }

        _pendingEvents.Enqueue(feedbackEvent);
        ScheduleDrain();
    }

    private void ScheduleDrain()
    {
        if (Interlocked.Exchange(ref _isDispatchScheduled, 1) == 1)
        {
            return;
        }

        _application.Dispatcher.BeginInvoke(DrainPendingEvents, DispatcherPriority.Render);
    }

    private void DrainPendingEvents()
    {
        try
        {
            while (_pendingEvents.TryDequeue(out var feedbackEvent))
            {
                Apply(feedbackEvent);
            }
        }
        catch (InvalidOperationException)
        {
            _isFeedbackDisabled = true;
            _window?.Close();
            _window = null;
            _pendingEvents.Clear();
        }
        finally
        {
            Interlocked.Exchange(ref _isDispatchScheduled, 0);
            if (!_pendingEvents.IsEmpty && !_isFeedbackDisabled)
            {
                ScheduleDrain();
            }
        }
    }

    private void Apply(FeedbackEvent feedbackEvent)
    {
        switch (feedbackEvent.Kind)
        {
            case FeedbackEventKind.Begin:
                _window ??= new GestureTrailWindow();
                _window.Begin(feedbackEvent.Point);
                break;
            case FeedbackEventKind.Point:
                _window?.AddPoint(feedbackEvent.Point);
                break;
            case FeedbackEventKind.End:
                _window?.End();
                break;
        }
    }

    private readonly record struct FeedbackEvent(FeedbackEventKind Kind, GesturePoint Point)
    {
        public static FeedbackEvent Begin(GesturePoint point) => new(FeedbackEventKind.Begin, point);

        public static FeedbackEvent Move(GesturePoint point) => new(FeedbackEventKind.Point, point);

        public static FeedbackEvent End() => new(FeedbackEventKind.End, new GesturePoint(0, 0));
    }

    private enum FeedbackEventKind
    {
        Begin,
        Point,
        End
    }

    private sealed class GestureTrailWindow : Window
    {
        private const int GwlExStyle = -20;
        private const int WsExNoActivate = 0x08000000;
        private const int WsExToolWindow = 0x00000080;
        private const int WsExTransparent = 0x00000020;

        private readonly GestureTrailVisual _visual = new();

        public GestureTrailWindow()
        {
            AllowsTransparency = true;
            Background = MediaBrushes.Transparent;
            ShowInTaskbar = false;
            ShowActivated = false;
            Focusable = false;
            Topmost = true;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            IsHitTestVisible = false;
            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            // Keep the transparent overlay one device-independent pixel short of
            // the virtual-screen bounds. Windows otherwise classifies this helper
            // window as a full-screen app and automatically enables Do Not Disturb.
            Width = Math.Max(1, SystemParameters.VirtualScreenWidth - 1);
            Height = Math.Max(1, SystemParameters.VirtualScreenHeight - 1);
            Content = _visual;
            SourceInitialized += (_, _) => EnableClickThrough();
        }

        public void Begin(GesturePoint point)
        {
            if (!IsVisible)
            {
                Show();
            }

            _visual.Reset(ToLocal(point));
        }

        public void AddPoint(GesturePoint point)
        {
            _visual.AddPoint(ToLocal(point));
        }

        public void End()
        {
            Hide();
        }

        private WpfPoint ToLocal(GesturePoint point)
        {
            return PointFromScreen(new WpfPoint(point.X, point.Y));
        }

        private void EnableClickThrough()
        {
            var handle = new WindowInteropHelper(this).Handle;
            var currentStyle = GetWindowLong(handle, GwlExStyle);
            SetWindowLong(
                handle,
                GwlExStyle,
                currentStyle | WsExTransparent | WsExNoActivate | WsExToolWindow);
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }

    private sealed class GestureTrailVisual : FrameworkElement
    {
        private readonly List<WpfPoint> _points = [];
        private readonly MediaPen _trailPen = new(new SolidColorBrush(MediaColor.FromRgb(45, 145, 255)), 5)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round
        };

        public void Reset(WpfPoint point)
        {
            _points.Clear();
            _points.Add(point);
            InvalidateVisual();
        }

        public void AddPoint(WpfPoint point)
        {
            if (_points.Count == 0 || Distance(_points[^1], point) >= 3)
            {
                _points.Add(point);
                InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (_points.Count < 2)
            {
                return;
            }

            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(_points[0], isFilled: false, isClosed: false);
                context.PolyLineTo(_points.Skip(1).ToArray(), isStroked: true, isSmoothJoin: true);
            }

            geometry.Freeze();
            drawingContext.DrawGeometry(null, _trailPen, geometry);
        }

        private static double Distance(WpfPoint first, WpfPoint second)
        {
            var deltaX = first.X - second.X;
            var deltaY = first.Y - second.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }
    }
}
