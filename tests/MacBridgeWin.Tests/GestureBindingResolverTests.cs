using MacBridgeWin.Core.Configuration;
using MacBridgeWin.Core.Gestures;

namespace MacBridgeWin.Tests;

[TestClass]
public class GestureBindingResolverTests
{
    [TestMethod]
    public void UnconfiguredAndEmptyGesturesPreserveRightClick()
    {
        var configuration = new MouseGestureConfiguration();
        Assert.IsNull(GestureBindingResolver.Resolve(configuration, [GestureDirection.Left]));
        Assert.IsNull(GestureBindingResolver.Resolve(configuration, []));
    }

    [TestMethod]
    public void DisabledAndNoneActionsPreserveRightClick()
    {
        var configuration = new MouseGestureConfiguration
        {
            Gestures = [new("Left", "None"), new("Right", "KeyboardShortcut:Ctrl+Tab", false)]
        };
        Assert.IsNull(GestureBindingResolver.Resolve(configuration, [GestureDirection.Left]));
        Assert.IsNull(GestureBindingResolver.Resolve(configuration, [GestureDirection.Right]));
    }

    [TestMethod]
    public void ConfiguredGestureIsConsumed()
    {
        var binding = new GestureActionConfiguration("Left", "KeyboardShortcut:Ctrl+Tab");
        var configuration = new MouseGestureConfiguration { Gestures = [binding] };
        Assert.AreSame(binding, GestureBindingResolver.Resolve(configuration, [GestureDirection.Left]));
    }
}
