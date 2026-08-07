using MacBridgeWin.Core.Gestures;

namespace MacBridgeWin.Tests;

[TestClass]
public sealed class BasicDirectionalGestureRecognizerTests
{
    private readonly BasicDirectionalGestureRecognizer _recognizer = new();

    [TestMethod]
    public void Recognize_ReturnsLeft_WhenMovementIsMostlyLeft()
    {
        var result = _recognizer.Recognize([new(100, 100), new(20, 110)], thresholdPixels: 40);

        Assert.IsTrue(result.IsRecognized);
        Assert.AreEqual(GestureDirection.Left, result.Direction);
    }

    [TestMethod]
    public void Recognize_ReturnsRight_WhenMovementIsMostlyRight()
    {
        var result = _recognizer.Recognize([new(20, 100), new(90, 108)], thresholdPixels: 40);

        Assert.IsTrue(result.IsRecognized);
        Assert.AreEqual(GestureDirection.Right, result.Direction);
    }

    [TestMethod]
    public void Recognize_ReturnsUp_WhenMovementIsMostlyUp()
    {
        var result = _recognizer.Recognize([new(100, 100), new(108, 30)], thresholdPixels: 40);

        Assert.IsTrue(result.IsRecognized);
        Assert.AreEqual(GestureDirection.Up, result.Direction);
    }

    [TestMethod]
    public void Recognize_ReturnsDown_WhenMovementIsMostlyDown()
    {
        var result = _recognizer.Recognize([new(100, 30), new(108, 100)], thresholdPixels: 40);

        Assert.IsTrue(result.IsRecognized);
        Assert.AreEqual(GestureDirection.Down, result.Direction);
    }

    [TestMethod]
    public void Recognize_ReturnsNone_WhenMovementIsBelowThreshold()
    {
        var result = _recognizer.Recognize([new(100, 100), new(120, 110)], thresholdPixels: 40);

        Assert.IsFalse(result.IsRecognized);
    }

    [TestMethod]
    public void Recognize_ReturnsDiagonal_WhenMovementIsDiagonal()
    {
        var result = _recognizer.Recognize([new(100, 100), new(150, 145)], thresholdPixels: 40);

        Assert.IsTrue(result.IsRecognized);
        Assert.AreEqual(GestureDirection.DownRight, result.Direction);
    }

    [TestMethod]
    public void RecognizeSequence_ReturnsMultipleDirections()
    {
        var result = _recognizer.RecognizeSequence(
            [new(100, 100), new(40, 100), new(40, 40)],
            thresholdPixels: 40);

        CollectionAssert.AreEqual(
            new[] { GestureDirection.Left, GestureDirection.Up },
            result.ToArray());
    }

    [TestMethod]
    public void RecognizeSequence_ReturnsDiagonalPolylineDirections()
    {
        var result = _recognizer.RecognizeSequence(
            [new(100, 100), new(150, 150), new(200, 100), new(150, 50)],
            thresholdPixels: 32);

        CollectionAssert.AreEqual(
            new[] { GestureDirection.DownRight, GestureDirection.UpRight, GestureDirection.UpLeft },
            result.ToArray());
    }

    [TestMethod]
    public void GestureSequence_TryParse_AcceptsCommaSeparatedAndCompactSequences()
    {
        var commaSeparated = GestureSequence.TryParse("DownRight,UpLeft", out var first);
        var compact = GestureSequence.TryParse("DownRightUpLeft", out var second);

        Assert.IsTrue(commaSeparated);
        Assert.IsTrue(compact);
        CollectionAssert.AreEqual(first.ToArray(), second.ToArray());
    }

    [TestMethod]
    public void GestureSequence_IsCompatible_ExpandsRecognizedDiagonalTurns()
    {
        GestureSequence.TryParse("Down,Left", out var configured);
        GestureSequence.TryParse("DownLeft", out var recognizedDiagonal);
        GestureSequence.TryParse("Down,DownLeft,Left", out var recognizedTurn);

        Assert.IsTrue(GestureSequence.IsCompatible(configured, recognizedDiagonal));
        Assert.IsTrue(GestureSequence.IsCompatible(configured, recognizedTurn));
    }

    [TestMethod]
    public void GestureSequence_IsCompatible_AcceptsNoisyContinuousCrossX()
    {
        GestureSequence.TryParse("DownLeft,Up,DownRight", out var configured);
        GestureSequence.TryParse("Down,DownLeft,Up,Right,DownRight", out var recognized);

        Assert.IsTrue(GestureSequence.IsCompatible(configured, recognized));
    }

    [TestMethod]
    public void GestureSequence_IsCompatible_AcceptsContinuousCrossXWhenFirstDiagonalIsCapturedAsDown()
    {
        GestureSequence.TryParse("DownLeft,Up,DownRight", out var configured);
        GestureSequence.TryParse("Down,Up,DownRight", out var recognized);

        Assert.IsTrue(GestureSequence.IsCompatible(configured, recognized));
    }

    [TestMethod]
    public void GestureSequence_IsCompatible_AcceptsObservedVDownShape()
    {
        GestureSequence.TryParse("DownLeft,DownRight", out var configured);
        GestureSequence.TryParse("Left,DownRight,Right", out var recognized);

        Assert.IsTrue(GestureSequence.IsCompatible(configured, recognized));
    }

    [TestMethod]
    public void GestureSequence_IsCompatible_AcceptsGeometricVDownShape()
    {
        GestureSequence.TryParse("DownLeft,DownRight", out var configured);
        GestureSequence.TryParse("DownRight,UpRight", out var recognized);

        Assert.IsTrue(GestureSequence.IsCompatible(configured, recognized));
    }

    [TestMethod]
    public void GestureSequence_IsCompatible_AcceptsSampledExistingVDownShape()
    {
        GestureSequence.TryParse("DownLeft,DownRight", out var configured);
        GestureSequence.TryParse("DownLeft,Down,DownRight", out var recognized);

        Assert.IsTrue(GestureSequence.IsCompatible(configured, recognized));
    }
}
