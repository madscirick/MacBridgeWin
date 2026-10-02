using System.Reflection;
using MacBridgeWin.App.Services.Mouse;
using MacBridgeWin.Core.Gestures;

namespace MacBridgeWin.Tests;

[TestClass]
public class MouseRightClickTests
{
    [TestMethod]
    public void FirstFeedbackPointIsQueuedOnlyOnce()
    {
        var moves = 0;
        using var hook = new MouseHookService(_ => GestureRecognitionResult.None, _ => { }, (_, _) => { }, _ => { }, _ => moves++, () => { });
        typeof(MouseHookService).GetMethod("CaptureMove", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(hook, [new GesturePoint(12, 0)]);
        Assert.AreEqual(1, moves);
    }

    [DataTestMethod]
    [DataRow(12, 12, 48, false)]
    [DataRow(47, 0, 48, false)]
    [DataRow(48, 0, 48, true)]
    [DataRow(0, -48, 48, true)]
    [DataRow(12, 0, 10, true)]
    public void ThresholdPrefilterPreservesSmallProfileGestures(int x, int y, int threshold, bool expected)
    {
        Assert.AreEqual(expected, BasicDirectionalGestureRecognizer.HasThresholdMovement(
            [new(0, 0), new(x, y), new(0, 0)], threshold));
    }

    [DataTestMethod]
    [DataRow(0, false, 1, 0)]
    [DataRow(12, false, 1, 1)]
    [DataRow(60, false, 1, 1)]
    [DataRow(60, true, 0, 1)]
    public void ReleaseRestoresClickUnlessGestureHasAction(int distance, bool recognized, int expectedClicks, int expectedRecognitions)
    {
        int clicks = 0, recognitions = 0, executions = 0;
        using var hook = new MouseHookService(_ =>
        {
            recognitions++;
            return recognized ? GestureRecognitionResult.Recognized(GestureDirection.Right) : GestureRecognitionResult.None;
        }, _ => executions++, (_, _) => clicks++, _ => { }, _ => { }, () => { });
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        typeof(MouseHookService).GetMethod("CaptureMove", flags)!.Invoke(hook, [new GesturePoint(distance, 0)]);
        typeof(MouseHookService).GetMethod("FinishCapture", flags)!.Invoke(hook, [new GesturePoint(distance, 0)]);
        Assert.AreEqual(expectedClicks, clicks);
        Assert.AreEqual(expectedRecognitions, recognitions);
        Assert.AreEqual(recognized ? 1 : 0, executions);
    }

    [TestMethod]
    public void HookThreadCanStartStopAndRestart()
    {
        if (!OperatingSystem.IsWindows()) Assert.Inconclusive("Windows hook requires Windows.");
        using var hook = new MouseHookService(_ => GestureRecognitionResult.None, _ => { }, (_, _) => { }, _ => { }, _ => { }, () => { });
        hook.Start();
        hook.Stop();
        hook.Start();
        hook.Stop();
    }
}
