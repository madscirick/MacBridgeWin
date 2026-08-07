using MacBridgeWin.Core.Configuration;

namespace MacBridgeWin.Tests;

[TestClass]
public sealed class ConfigurationDefaultsTests
{
    [TestMethod]
    public void Create_IncludesInitialKeyboardMappings()
    {
        var configuration = ConfigurationDefaults.Create();

        Assert.AreEqual(ConfigurationDefaults.CurrentSchemaVersion, configuration.SchemaVersion);
        Assert.IsFalse(configuration.Features.KeyboardMappingsEnabled);
        Assert.IsFalse(configuration.Features.MouseGesturesEnabled);
        Assert.IsFalse(configuration.Startup.StartWithWindows);
        Assert.IsTrue(configuration.Keyboard.Mappings.Any(mapping =>
            mapping.SourceShortcut == "Alt+C" && mapping.TargetShortcut == "Ctrl+C"));
        Assert.IsTrue(configuration.Keyboard.Mappings.Any(mapping =>
            mapping.SourceShortcut == "Alt+Shift+Z" && mapping.TargetShortcut == "Ctrl+Y"));
    }

    [TestMethod]
    public void Create_IncludesRecommendedMouseGestureActions()
    {
        var configuration = ConfigurationDefaults.Create();

        Assert.AreEqual(48, configuration.MouseGestures.MovementThresholdPixels);
        CollectionAssert.AreEquivalent(
            new[]
            {
                new GestureActionConfiguration("DownLeft,DownRight", GestureActionCatalog.OpenTerminal),
                new GestureActionConfiguration("DownLeft,Up,DownRight", "KeyboardShortcut:Alt+F4"),
                new GestureActionConfiguration("Left", "None"),
                new GestureActionConfiguration("Right", "None"),
                new GestureActionConfiguration("Up", "None"),
                new GestureActionConfiguration("Down", "KeyboardShortcut:Alt+F4")
            },
            configuration.MouseGestures.Gestures);
        Assert.IsTrue(configuration.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "DownLeft,DownRight" && gesture.Action == GestureActionCatalog.OpenTerminal));
    }

    [TestMethod]
    public void Create_IncludesRecommendedApplicationProfiles()
    {
        var configuration = ConfigurationDefaults.Create();

        var chrome = configuration.Profiles.Single(profile => profile.ProcessName == "chrome.exe");
        Assert.AreEqual("Google Chrome", chrome.Name);
        Assert.IsTrue(chrome.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "Left" && gesture.Action == "KeyboardShortcut:Ctrl+Shift+Tab"));
        Assert.IsTrue(chrome.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "Right" && gesture.Action == "KeyboardShortcut:Ctrl+Tab"));
        Assert.IsTrue(chrome.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "Up" && gesture.Action == GestureActionCatalog.ScrollToTop));
        Assert.IsTrue(chrome.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "Down" && gesture.Action == GestureActionCatalog.ScrollToBottom));
        Assert.IsTrue(chrome.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "DownLeft,Up,DownRight" && gesture.Action == "KeyboardShortcut:Ctrl+W"));

        var acrobat = configuration.Profiles.Single(profile => profile.ProcessName == "Acrobat.exe");
        Assert.AreEqual("Adobe Acrobat", acrobat.Name);
        Assert.IsTrue(acrobat.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "Right" && gesture.Action == "KeyboardShortcut:Ctrl+Tab"));

        var reader = configuration.Profiles.Single(profile => profile.ProcessName == "AcroRd32.exe");
        Assert.AreEqual("Adobe Acrobat Reader", reader.Name);
        Assert.IsTrue(reader.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "Right" && gesture.Action == "KeyboardShortcut:Ctrl+Tab"));
    }

    [TestMethod]
    public void ApplyRecommendedMouseGestureActions_UpgradesInitialPlaceholders()
    {
        var configuration = ConfigurationDefaults.Create();
        configuration.MouseGestures.Gestures =
        [
            new("Left", "None"),
            new("Right", "None"),
            new("Up", "None"),
            new("Down", "None")
        ];

        var changed = ConfigurationDefaults.ApplyRecommendedMouseGestureActions(configuration);

        Assert.IsTrue(changed);
        Assert.IsTrue(configuration.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "Down" && gesture.Action == "KeyboardShortcut:Alt+F4"));
        Assert.IsTrue(configuration.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "DownLeft,Up,DownRight" && gesture.Action == "KeyboardShortcut:Alt+F4"));
    }

    [TestMethod]
    public void ApplyRecommendedMouseGestureActions_UpgradesLegacyCrossGesture()
    {
        var configuration = ConfigurationDefaults.Create();
        var crossIndex = configuration.MouseGestures.Gestures.FindIndex(gesture =>
            gesture.Gesture == "DownLeft,Up,DownRight");
        configuration.MouseGestures.Gestures[crossIndex] = new GestureActionConfiguration(
            "DownRight,UpLeft,DownLeft,UpRight",
            "KeyboardShortcut:Alt+F4",
            ShowCursor: true,
            Note: "Close the active window");

        var changed = ConfigurationDefaults.ApplyRecommendedMouseGestureActions(configuration);

        Assert.IsTrue(changed);
        var cross = configuration.MouseGestures.Gestures.Single(gesture => gesture.Gesture == "DownLeft,Up,DownRight");
        Assert.IsTrue(cross.ShowCursor);
        Assert.AreEqual("Close the active window", cross.Note);
    }

    [TestMethod]
    public void ApplyRecommendedMouseGestureActions_UpgradesLegacyDefaultsAndAddsProfiles()
    {
        var configuration = ConfigurationDefaults.Create();
        configuration.Profiles.Clear();
        configuration.MouseGestures.Gestures =
        [
            new("Left", "KeyboardShortcut:Alt+Left"),
            new("Right", "KeyboardShortcut:Alt+Right"),
            new("Up", "KeyboardShortcut:Ctrl+T"),
            new("Down", "KeyboardShortcut:Ctrl+W")
        ];

        var changed = ConfigurationDefaults.ApplyRecommendedMouseGestureActions(configuration);

        Assert.IsTrue(changed);
        Assert.IsTrue(configuration.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "Down" && gesture.Action == "KeyboardShortcut:Alt+F4"));
        Assert.IsTrue(configuration.Profiles.Any(profile => profile.ProcessName == "chrome.exe"));
        Assert.IsTrue(configuration.Profiles.Any(profile => profile.ProcessName == "Acrobat.exe"));
        Assert.IsTrue(configuration.Profiles.Any(profile => profile.ProcessName == "AcroRd32.exe"));
    }

    [TestMethod]
    public void ApplyRecommendedMouseGestureActions_DoesNotDuplicateExistingProfiles()
    {
        var configuration = ConfigurationDefaults.Create();
        var initialChromeProfiles = configuration.Profiles.Count(profile => profile.ProcessName == "chrome.exe");

        ConfigurationDefaults.ApplyRecommendedMouseGestureActions(configuration);

        Assert.AreEqual(initialChromeProfiles, configuration.Profiles.Count(profile => profile.ProcessName == "chrome.exe"));
    }

    [TestMethod]
    public void ApplyRecommendedMouseGestureActions_AddsCloseTabCrossGestureToExistingChromeProfile()
    {
        var configuration = ConfigurationDefaults.Create();
        var chrome = configuration.Profiles.Single(profile => profile.ProcessName == "chrome.exe");
        chrome.MouseGestures.Gestures.RemoveAll(gesture => gesture.Gesture == "DownLeft,Up,DownRight");

        var changed = ConfigurationDefaults.ApplyRecommendedMouseGestureActions(configuration);

        Assert.IsTrue(changed);
        Assert.IsTrue(chrome.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "DownLeft,Up,DownRight" && gesture.Action == GestureActionCatalog.CloseTab));
    }

    [TestMethod]
    public void ApplyRecommendedMouseGestureActions_AddsPreviousTabGestureToExistingChromeProfile()
    {
        var configuration = ConfigurationDefaults.Create();
        var chrome = configuration.Profiles.Single(profile => profile.ProcessName == "chrome.exe");
        chrome.MouseGestures.Gestures.RemoveAll(gesture => gesture.Gesture == "Left");

        var changed = ConfigurationDefaults.ApplyRecommendedMouseGestureActions(configuration);

        Assert.IsTrue(changed);
        Assert.IsTrue(chrome.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "Left" && gesture.Action == GestureActionCatalog.PreviousTab));
    }

    [TestMethod]
    public void ApplyRecommendedMouseGestureActions_AddsScrollGesturesToExistingBrowserProfile()
    {
        var configuration = ConfigurationDefaults.Create();
        var edge = configuration.Profiles.Single(profile => profile.ProcessName == "msedge.exe");
        edge.MouseGestures.Gestures.RemoveAll(gesture => gesture.Gesture is "Up" or "Down");

        var changed = ConfigurationDefaults.ApplyRecommendedMouseGestureActions(configuration);

        Assert.IsTrue(changed);
        Assert.IsTrue(edge.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "Up" && gesture.Action == GestureActionCatalog.ScrollToTop));
        Assert.IsTrue(edge.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "Down" && gesture.Action == GestureActionCatalog.ScrollToBottom));
    }

    [TestMethod]
    public void ApplyRecommendedMouseGestureActions_UpgradesBrowserScrollShortcuts()
    {
        var configuration = ConfigurationDefaults.Create();
        var chrome = configuration.Profiles.Single(profile => profile.ProcessName == "chrome.exe");
        chrome.MouseGestures.Gestures.RemoveAll(gesture => gesture.Gesture is "Up" or "Down");
        chrome.MouseGestures.Gestures.Add(new("Up", "KeyboardShortcut:Home"));
        chrome.MouseGestures.Gestures.Add(new("Down", "KeyboardShortcut:End"));

        var changed = ConfigurationDefaults.ApplyRecommendedMouseGestureActions(configuration);

        Assert.IsTrue(changed);
        Assert.IsTrue(chrome.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "Up" && gesture.Action == "KeyboardShortcut:Ctrl+Home"));
        Assert.IsTrue(chrome.MouseGestures.Gestures.Any(gesture =>
            gesture.Gesture == "Down" && gesture.Action == "KeyboardShortcut:Ctrl+End"));
    }
}
