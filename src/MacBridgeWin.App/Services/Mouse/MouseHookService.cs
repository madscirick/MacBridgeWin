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
        if (_hookHandle != IntPtr.Zero)
        {
            return;
        }

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
        if (_hookHandle == IntPtr.Zero)
        {
            return;
        }

        UnhookWindowsHookEx(_hookHandle);
        _hookHandle = IntPtr.Zero;
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

        var hookInfo = Marshal.PtrToStructure<MsLlHookStruct>(lParam);
        var isSynthetic = (hookInfo.Flags & LlmhfInjected) != 0
            && hookInfo.DwExtraInfo == SyntheticMouseInputSender.ExtraInfoMarker;

        if (isSynthetic)
        {
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        var message = wParam.ToInt32();
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
        var result = _recognize(_points);
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

        if (!hadGestureMovement)
        {
            _sendRightClick(releasePoint.X, releasePoint.Y);
        }
    }

    private void CaptureMove(GesturePoint point)
    {
        if (!_hasGestureMovement && DistanceSquared(_startPoint, point) >= FeedbackStartDistancePixels * FeedbackStartDistancePixels)
        {
            _hasGestureMovement = true;
            _gestureStarted(_startPoint);
            _gestureMoved(point);
        }

        if (DistanceSquared(_lastCapturedPoint, point) >= MoveSampleDistancePixels * MoveSampleDistancePixels)
        {
            AddPoint(point);
            if (_hasGestureMovement)
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
