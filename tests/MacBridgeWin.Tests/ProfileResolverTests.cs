using MacBridgeWin.Core.Configuration;
using MacBridgeWin.Core.Profiles;

namespace MacBridgeWin.Tests;

[TestClass]
public sealed class ProfileResolverTests
{
    private readonly ProfileResolver _resolver = new();

    [TestMethod]
    public void Resolve_ReturnsGlobalConfiguration_WhenNoProfileMatches()
    {
        var configuration = ConfigurationDefaults.Create();

        var resolved = _resolver.Resolve(configuration, "notepad");

        Assert.IsNull(resolved.ProfileName);
        Assert.AreSame(configuration.Keyboard, resolved.Keyboard);
        Assert.AreSame(configuration.MouseGestures, resolved.MouseGestures);
    }

    [TestMethod]
    public void Resolve_ReturnsMatchingProfile_ByProcessName()
    {
        var configuration = ConfigurationDefaults.Create();
        configuration.Profiles.Clear();
        var profile = new ApplicationProfileConfiguration
        {
            Name = "Browser",
            ProcessName = "chrome.exe",
            Keyboard = new KeyboardConfiguration
            {
                Mappings = [new("Alt+L", "Ctrl+L")]
            },
            MouseGestures = new MouseGestureConfiguration
            {
                MovementThresholdPixels = 32,
                Gestures = [new("Left,Up", "KeyboardShortcut:Ctrl+T")]
            }
        };
        configuration.Profiles.Add(profile);

        var resolved = _resolver.Resolve(configuration, "chrome");

        Assert.AreEqual("Browser", resolved.ProfileName);
        Assert.AreSame(profile.Keyboard, resolved.Keyboard);
        Assert.AreEqual(32, resolved.MouseGestures.MovementThresholdPixels);
        Assert.IsTrue(resolved.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "Left,Up" && gesture.Action == "KeyboardShortcut:Ctrl+T"));
    }

    [TestMethod]
    public void Resolve_MergesProfileMouseGesturesOverGlobalGestures()
    {
        var configuration = ConfigurationDefaults.Create();
        configuration.Profiles.Clear();
        configuration.MouseGestures.Gestures =
        [
            new("Left", "None"),
            new("Right", "None"),
            new("Down", "KeyboardShortcut:Alt+F4")
        ];
        configuration.Profiles.Add(new ApplicationProfileConfiguration
        {
            Name = "Browser",
            ProcessName = "chrome.exe",
            Keyboard = configuration.Keyboard,
            MouseGestures = new MouseGestureConfiguration
            {
                MovementThresholdPixels = 32,
                Gestures =
                [
                    new("Left", "KeyboardShortcut:Alt+Left"),
                    new("Right", "KeyboardShortcut:Ctrl+Tab")
                ]
            }
        });

        var resolved = _resolver.Resolve(configuration, "chrome");

        Assert.AreEqual("Browser", resolved.ProfileName);
        Assert.IsTrue(resolved.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "Left" && gesture.Action == "KeyboardShortcut:Alt+Left"));
        Assert.IsTrue(resolved.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "Right" && gesture.Action == "KeyboardShortcut:Ctrl+Tab"));
        Assert.IsTrue(resolved.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "Down" && gesture.Action == "KeyboardShortcut:Alt+F4"));
    }
}
