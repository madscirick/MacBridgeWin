using MacBridgeWin.Core.Configuration;

namespace MacBridgeWin.Tests;

[TestClass]
public class ChromeNavigationDefaultsTests
{
    [TestMethod]
    public void NewChromeProfileIncludesBackAndForward()
    {
        var configuration = ConfigurationDefaults.Create();
        var chrome = configuration.Profiles.Single(profile => profile.ProcessName == "chrome.exe");
        Assert.AreEqual(GestureActionCatalog.BrowserBack,
            chrome.MouseGestures.Gestures.Single(gesture => gesture.Gesture == "Down,Left").Action);
        Assert.AreEqual(GestureActionCatalog.BrowserForward,
            chrome.MouseGestures.Gestures.Single(gesture => gesture.Gesture == "Down,Right").Action);
        Assert.IsFalse(configuration.Profiles.Where(profile => profile.ProcessName != "chrome.exe")
            .Any(profile => profile.MouseGestures.Gestures.Any(gesture => gesture.Gesture is "Down,Left" or "Down,Right")));
    }

    [TestMethod]
    public void ExistingChromeProfileReceivesMissingGesturesOnlyOnce()
    {
        var configuration = ConfigurationDefaults.Create();
        var chrome = configuration.Profiles.Single(profile => profile.ProcessName == "chrome.exe");
        chrome.MouseGestures.Gestures.RemoveAll(gesture => gesture.Gesture is "Down,Left" or "Down,Right");
        Assert.IsTrue(ConfigurationDefaults.ApplyRecommendedMouseGestureActions(configuration));
        Assert.IsFalse(ConfigurationDefaults.ApplyRecommendedMouseGestureActions(configuration));
        Assert.AreEqual(1, chrome.MouseGestures.Gestures.Count(gesture => gesture.Gesture == "Down,Left"));
        Assert.AreEqual(1, chrome.MouseGestures.Gestures.Count(gesture => gesture.Gesture == "Down,Right"));
    }

    [TestMethod]
    public void ExistingCustomAndDisabledBindingsArePreserved()
    {
        var configuration = ConfigurationDefaults.Create();
        var chrome = configuration.Profiles.Single(profile => profile.ProcessName == "chrome.exe");
        chrome.MouseGestures.Gestures.RemoveAll(gesture => gesture.Gesture is "Down,Left" or "Down,Right");
        var custom = new GestureActionConfiguration("Down,Left", GestureActionCatalog.NewTab, Note: "Custom");
        var disabled = new GestureActionConfiguration("Down,Right", GestureActionCatalog.None, Enabled: false);
        chrome.MouseGestures.Gestures.AddRange([custom, disabled]);
        ConfigurationDefaults.ApplyRecommendedMouseGestureActions(configuration);
        Assert.AreSame(custom, chrome.MouseGestures.Gestures.Single(gesture => gesture.Gesture == "Down,Left"));
        Assert.AreSame(disabled, chrome.MouseGestures.Gestures.Single(gesture => gesture.Gesture == "Down,Right"));
    }
}
