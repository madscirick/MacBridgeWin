using MacBridgeWin.Core.Gestures;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MacBridgeWin.App.Services.Mouse;

public sealed class MouseHookService : IDisposable
{
    private const int FeedbackStartDistancePixels = 8;
    private const int MoveSampleDistancePixels = 6;
    private const int MaxCapturedPoints = 512;
    private const int WhMouseLl = 14;
    private const int WmMouseMove = 0x0200;
    private const int WmRButtonDown = 0x0204;
    private const int WmRButtonUp = 0x0205;
    private const int LlmhfInjected = 0x00000001;

    private readonly LowLevelMouseProc _hookProc;
    private readonly Func<IReadOnlyList<GesturePoint>, GestureRecognitionResult> _recognize;
    private readonly Action<IReadOnlyList<GestureDirection>> _executeGesture;
    private readonly Action<int, int> _sendRightClick;
    private readonly Action<GesturePoint> _gestureStarted;
    private readonly Action<GesturePoint> _gestureMoved;
    private readonly Action _gestureEnded;
    private readonly List<GesturePoint> _points = [];
    private IntPtr _hookHandle;
    private Thread? _thread;
    private uint _threadId;
    private bool _isCapturing;
    private bool _hasGestureMovement;
    private GesturePoint _startPoint;
    private GesturePoint _lastCapturedPoint;

    public MouseHookService(
        Func<IReadOnlyList<GesturePoint>, GestureRecognitionResult> recognize,
        Action<IReadOnlyList<GestureDirection>> executeGesture,
        Action<int, int> sendRightClick,
        Action<GesturePoint> gestureStarted,
        Action<GesturePoint> gestureMoved,
        Action gestureEnded)
    {
        _recognize = recognize;
        _executeGesture = executeGesture;
        _sendRightClick = sendRightClick;
        _gestureStarted = gestureStarted;
        _gestureMoved = gestureMoved;
        _gestureEnded = gestureEnded;
        _hookProc = HookCallback;
    }

    public void Start()
    {
        if (_thread is not null)
        {
            return;
        }

        using var ready = new ManualResetEventSlim();
        Exception? failure = null;
        _thread = new Thread(() =>
        {
            try
            {
                _threadId = GetCurrentThreadId();
                PeekMessage(out _, IntPtr.Zero, 0, 0, 0);
                InstallHook();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
            finally
            {
                ready.Set();
            }

            if (failure is not null) return;
            try
            {
                while (GetMessage(out var message, IntPtr.Zero, 0, 0) > 0)
                {
                    TranslateMessage(ref message);
                    DispatchMessage(ref message);
                }
            }
            finally
            {
                UnhookWindowsHookEx(_hookHandle);
                _hookHandle = IntPtr.Zero;
            }
        }) { IsBackground = true, Name = "MacBridgeWin mouse hook" };
        _thread.Start();
        ready.Wait();
        if (failure is not null)
        {
            _thread.Join();
            _thread = null;
            throw new InvalidOperationException("Failed to start the mouse hook thread.", failure);
        }
    }

    private void InstallHook()
    {

        using var currentProcess = Process.GetCurrentProcess();
        using var currentModule = currentProcess.MainModule;
        var moduleHandle = currentModule is null ? IntPtr.Zero : GetModuleHandle(currentModule.ModuleName);
        _hookHandle = SetWindowsHookEx(WhMouseLl, _hookProc, moduleHandle, 0);

        if (_hookHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to install the low-level mouse hook.");
        }
    }

    public void Stop()
    {
        if (_thread is null)
        {
            return;
        }

        if (_thread.IsAlive)
        {
            PostThreadMessage(_threadId, 0x0012, UIntPtr.Zero, IntPtr.Zero);
            _thread.Join();
        }
        _thread = null;
        _threadId = 0;
        _gestureEnded();
        _isCapturing = false;
        _hasGestureMovement = false;
        _points.Clear();
    }

    public void Dispose()
    {
        Stop();
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode < 0)
        {
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        var message = wParam.ToInt32();
        // Most global notifications are ordinary movement or unrelated buttons.
        // Avoid marshaling their native payload when no gesture is being captured.
        if (message != WmRButtonDown && !(_isCapturing && (message == WmMouseMove || message == WmRButtonUp)))
        {
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        var hookInfo = Marshal.PtrToStructure<MsLlHookStruct>(lParam);
        var isSynthetic = (hookInfo.Flags & LlmhfInjected) != 0
            && hookInfo.DwExtraInfo == SyntheticMouseInputSender.ExtraInfoMarker;

        if (isSynthetic)
        {
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        var point = new GesturePoint(hookInfo.Point.X, hookInfo.Point.Y);

        switch (message)
        {
            case WmRButtonDown:
                _isCapturing = true;
                _hasGestureMovement = false;
                _startPoint = point;
                _lastCapturedPoint = point;
                _points.Clear();
                _points.Add(point);
                return new IntPtr(1);

            case WmMouseMove when _isCapturing:
                CaptureMove(point);
                return CallNextHookEx(_hookHandle, nCode, wParam, lParam);

            case WmRButtonUp when _isCapturing:
                AddPoint(point);
                FinishCapture(point);
                return new IntPtr(1);

            default:
                return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }
    }

    private void FinishCapture(GesturePoint releasePoint)
    {
        _isCapturing = false;
        // Ordinary clicks need no process lookup or profile resolution.
        var result = _hasGestureMovement ? _recognize(_points) : GestureRecognitionResult.None;
        var hadGestureMovement = _hasGestureMovement;
        _points.Clear();
        _hasGestureMovement = false;
        if (hadGestureMovement)
        {
            _gestureEnded();
        }

        if (result.IsRecognized && result.Sequence.Count > 0)
        {
            _executeGesture(result.Sequence);
            return;
        }

        _sendRightClick(releasePoint.X, releasePoint.Y);
    }

    private void CaptureMove(GesturePoint point)
    {
        var started = false;
        if (!_hasGestureMovement && DistanceSquared(_startPoint, point) >= FeedbackStartDistancePixels * FeedbackStartDistancePixels)
        {
            _hasGestureMovement = true;
            _gestureStarted(_startPoint);
            _gestureMoved(point);
            started = true;
        }

        if (DistanceSquared(_lastCapturedPoint, point) >= MoveSampleDistancePixels * MoveSampleDistancePixels)
        {
            AddPoint(point);
            if (_hasGestureMovement && !started)
            {
                _gestureMoved(point);
            }
        }
    }

    private void AddPoint(GesturePoint point)
    {
        if (_points.Count > 0 && _points[^1] == point)
        {
            return;
        }

        if (_points.Count >= MaxCapturedPoints)
        {
            _points.RemoveAt(1);
        }

        _points.Add(point);
        _lastCapturedPoint = point;
    }

    private static int DistanceSquared(GesturePoint first, GesturePoint second)
    {
        var deltaX = first.X - second.X;
        var deltaY = first.Y - second.Y;
        return (deltaX * deltaX) + (deltaY * deltaY);
    }

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct Message
    {
        public IntPtr Hwnd;
        public uint Id;
        public UIntPtr WParam;
        public IntPtr LParam;
        public uint Time;
        public Point Point;
        public uint Private;
    }

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();
    [DllImport("user32.dll")]
    private static extern bool PeekMessage(out Message message, IntPtr hwnd, uint min, uint max, uint remove);
    [DllImport("user32.dll")]
    private static extern int GetMessage(out Message message, IntPtr hwnd, uint min, uint max);
    [DllImport("user32.dll")]
    private static extern bool TranslateMessage(ref Message message);
    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage(ref Message message);
    [DllImport("user32.dll")]
    private static extern bool PostThreadMessage(uint threadId, uint message, UIntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct Point
    {
        public readonly int X;
        public readonly int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct MsLlHookStruct
    {
        public readonly Point Point;
        public readonly uint MouseData;
        public readonly uint Flags;
        public readonly uint Time;
        public readonly UIntPtr DwExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}
