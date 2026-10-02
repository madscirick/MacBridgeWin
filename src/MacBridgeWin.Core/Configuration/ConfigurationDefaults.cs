namespace MacBridgeWin.Core.Configuration;

public static class ConfigurationDefaults
{
    public const int CurrentSchemaVersion = 2;
    private const int DefaultMouseGestureThresholdPixels = 48;
    private const string LeftGesture = "Left";
    private const string RightGesture = "Right";
    private const string UpGesture = "Up";
    private const string DownGesture = "Down";
    private const string CrossGesture = "DownLeft,Up,DownRight";
    private const string VDownGesture = "DownLeft,DownRight";
    private const string LegacyCrossGesture = "DownRight,UpLeft,DownLeft,UpRight";

    public static AppConfiguration Create()
    {
        return new AppConfiguration
        {
            SchemaVersion = CurrentSchemaVersion,
            Features = new FeatureConfiguration
            {
                KeyboardMappingsEnabled = false,
                MouseGesturesEnabled = false
            },
            Startup = new StartupConfiguration
            {
                StartWithWindows = false
            },
            Keyboard = CreateKeyboardConfiguration(),
            MouseGestures = new MouseGestureConfiguration
            {
                MovementThresholdPixels = DefaultMouseGestureThresholdPixels,
                Gestures = CreateGlobalMouseGestureActions()
            },
            Profiles = CreateRecommendedProfiles()
        };
    }

    public static bool ApplyRecommendedMouseGestureActions(AppConfiguration configuration)
    {
        var changed = false;

        if (HasInitialMouseGesturePlaceholders(configuration.MouseGestures)
            || HasLegacyMouseGestureDefaults(configuration.MouseGestures))
        {
            configuration.MouseGestures.Gestures = CreateGlobalMouseGestureActions();
            changed = true;
        }

        changed |= UpgradeLegacyCrossGesture(configuration.MouseGestures);
        changed |= AddMissingGlobalGestureAction(configuration.MouseGestures, CrossGesture, GestureActionCatalog.CloseWindow);
        changed |= AddMissingGlobalGestureAction(configuration.MouseGestures, VDownGesture, GestureActionCatalog.OpenTerminal);
        changed |= AddMissingRecommendedProfiles(configuration);
        changed |= AddMissingBrowserGestureActions(configuration);
        return changed;
    }

    private static List<GestureActionConfiguration> CreateGlobalMouseGestureActions()
    {
        return
        [
            new(VDownGesture, GestureActionCatalog.OpenTerminal),
            new(CrossGesture, GestureActionCatalog.CloseWindow),
            new(LeftGesture, GestureActionCatalog.None),
            new(RightGesture, GestureActionCatalog.None),
            new(UpGesture, GestureActionCatalog.None),
            new(DownGesture, GestureActionCatalog.CloseWindow)
        ];
    }

    private static bool AddMissingGlobalGestureAction(
        MouseGestureConfiguration mouseGestures,
        string gesture,
        string action)
    {
        if (mouseGestures.Gestures.Any(existing =>
            string.Equals(existing.Gesture, gesture, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        mouseGestures.Gestures.Insert(0, new GestureActionConfiguration(gesture, action));
        return true;
    }

    private static bool UpgradeLegacyCrossGesture(MouseGestureConfiguration mouseGestures)
    {
        var legacyIndex = mouseGestures.Gestures.FindIndex(existing =>
            string.Equals(existing.Gesture, LegacyCrossGesture, StringComparison.OrdinalIgnoreCase));
        if (legacyIndex < 0)
        {
            return false;
        }

        if (mouseGestures.Gestures.Any(existing =>
            string.Equals(existing.Gesture, CrossGesture, StringComparison.OrdinalIgnoreCase)))
        {
            mouseGestures.Gestures.RemoveAt(legacyIndex);
            return true;
        }

        mouseGestures.Gestures[legacyIndex] = mouseGestures.Gestures[legacyIndex] with { Gesture = CrossGesture };
        return true;
    }

    private static KeyboardConfiguration CreateKeyboardConfiguration()
    {
        return new KeyboardConfiguration
        {
            Mappings =
            [
                new("Alt+C", "Ctrl+C"),
                new("Alt+V", "Ctrl+V"),
                new("Alt+X", "Ctrl+X"),
                new("Alt+A", "Ctrl+A"),
                new("Alt+Z", "Ctrl+Z"),
                new("Alt+Shift+Z", "Ctrl+Y"),
                new("Alt+S", "Ctrl+S"),
                new("Alt+F", "Ctrl+F"),
                new("Alt+P", "Ctrl+P"),
                new("Alt+N", "Ctrl+N"),
                new("Alt+O", "Ctrl+O"),
                new("Alt+W", "Ctrl+W"),
                new("Alt+T", "Ctrl+T"),
                new("Alt+R", "Ctrl+R")
            ]
        };
    }

    private static List<ApplicationProfileConfiguration> CreateRecommendedProfiles()
    {
        return
        [
            CreateBrowserProfile("Google Chrome", "chrome.exe"),
            CreateBrowserProfile("Microsoft Edge", "msedge.exe"),
            CreateBrowserProfile("Mozilla Firefox", "firefox.exe"),
            CreateAdobeAcrobatProfile("Adobe Acrobat", "Acrobat.exe"),
            CreateAdobeAcrobatProfile("Adobe Acrobat Reader", "AcroRd32.exe")
        ];
    }

    private static ApplicationProfileConfiguration CreateBrowserProfile(string name, string processName)
    {
        var profile = new ApplicationProfileConfiguration
        {
            Name = name,
            ProcessName = processName,
            Keyboard = CreateKeyboardConfiguration(),
            MouseGestures = new MouseGestureConfiguration
            {
                MovementThresholdPixels = DefaultMouseGestureThresholdPixels,
                Gestures =
                [
                    new(LeftGesture, GestureActionCatalog.PreviousTab),
                    new(RightGesture, GestureActionCatalog.NextTab),
                    new(UpGesture, GestureActionCatalog.ScrollToTop),
                    new(DownGesture, GestureActionCatalog.ScrollToBottom),
                    new(CrossGesture, GestureActionCatalog.CloseTab)
                ]
            }
        };
        AddMissingChromeNavigationGestures(profile);
        return profile;
    }

    private static bool AddMissingChromeNavigationGestures(ApplicationProfileConfiguration profile)
    {
        if (!string.Equals(profile.ProcessName, "chrome.exe", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var changed = AddMissingProfileGestureAction(profile, "Down,Left", GestureActionCatalog.BrowserBack);
        changed |= AddMissingProfileGestureAction(profile, "Down,Right", GestureActionCatalog.BrowserForward);
        return changed;
    }

    private static bool AddMissingBrowserGestureActions(AppConfiguration configuration)
    {
        var changed = false;
        var browserProcessNames = new[] { "chrome.exe", "msedge.exe", "firefox.exe" };
        foreach (var profile in configuration.Profiles.Where(profile => browserProcessNames.Any(processName =>
                     string.Equals(profile.ProcessName, processName, StringComparison.OrdinalIgnoreCase))))
        {
            changed |= UpgradeBrowserScrollAction(profile, UpGesture, "KeyboardShortcut:Home", GestureActionCatalog.ScrollToTop);
            changed |= UpgradeBrowserScrollAction(profile, DownGesture, "KeyboardShortcut:End", GestureActionCatalog.ScrollToBottom);
            changed |= AddMissingProfileGestureAction(profile, LeftGesture, GestureActionCatalog.PreviousTab);
            changed |= AddMissingProfileGestureAction(profile, RightGesture, GestureActionCatalog.NextTab);
            changed |= AddMissingProfileGestureAction(profile, UpGesture, GestureActionCatalog.ScrollToTop);
            changed |= AddMissingProfileGestureAction(profile, DownGesture, GestureActionCatalog.ScrollToBottom);
            changed |= AddMissingProfileGestureAction(profile, CrossGesture, GestureActionCatalog.CloseTab);
            changed |= AddMissingChromeNavigationGestures(profile);
        }

        return changed;
    }

    private static bool UpgradeBrowserScrollAction(
        ApplicationProfileConfiguration profile,
        string gesture,
        string oldAction,
        string newAction)
    {
        var index = profile.MouseGestures.Gestures.FindIndex(existing =>
            string.Equals(existing.Gesture, gesture, StringComparison.OrdinalIgnoreCase)
            && string.Equals(existing.Action, oldAction, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return false;
        }

        profile.MouseGestures.Gestures[index] = profile.MouseGestures.Gestures[index] with { Action = newAction };
        return true;
    }

    private static bool AddMissingProfileGestureAction(
        ApplicationProfileConfiguration profile,
        string gesture,
        string action)
    {
        if (profile.MouseGestures.Gestures.Any(existing =>
            string.Equals(existing.Gesture, gesture, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        profile.MouseGestures.Gestures.Add(new GestureActionConfiguration(gesture, action));
        return true;
    }

    private static ApplicationProfileConfiguration CreateAdobeAcrobatProfile(string name, string processName)
    {
        return new ApplicationProfileConfiguration
        {
            Name = name,
            ProcessName = processName,
            Keyboard = CreateKeyboardConfiguration(),
            MouseGestures = new MouseGestureConfiguration
            {
                MovementThresholdPixels = DefaultMouseGestureThresholdPixels,
                Gestures =
                [
                    new(RightGesture, GestureActionCatalog.NextTab)
                ]
            }
        };
    }

    private static bool HasInitialMouseGesturePlaceholders(MouseGestureConfiguration mouseGestures)
    {
        if (mouseGestures.Gestures.Count != 4)
        {
            return false;
        }

        var actionsByGesture = mouseGestures.Gestures.ToDictionary(
            gesture => gesture.Gesture,
            gesture => gesture.Action,
            StringComparer.OrdinalIgnoreCase);

        return IsNone(actionsByGesture, LeftGesture)
            && IsNone(actionsByGesture, RightGesture)
            && IsNone(actionsByGesture, UpGesture)
            && IsNone(actionsByGesture, DownGesture);
    }

    private static bool HasLegacyMouseGestureDefaults(MouseGestureConfiguration mouseGestures)
    {
        if (mouseGestures.Gestures.Count != 4)
        {
            return false;
        }

        var actionsByGesture = mouseGestures.Gestures.ToDictionary(
            gesture => gesture.Gesture,
            gesture => gesture.Action,
            StringComparer.OrdinalIgnoreCase);

        return IsAction(actionsByGesture, LeftGesture, GestureActionCatalog.BrowserBack)
            && IsAction(actionsByGesture, RightGesture, "KeyboardShortcut:Alt+Right")
            && IsAction(actionsByGesture, UpGesture, "KeyboardShortcut:Ctrl+T")
            && IsAction(actionsByGesture, DownGesture, "KeyboardShortcut:Ctrl+W");
    }

    private static bool AddMissingRecommendedProfiles(AppConfiguration configuration)
    {
        var changed = false;
        foreach (var profile in CreateRecommendedProfiles())
        {
            if (configuration.Profiles.Any(existing =>
                string.Equals(existing.ProcessName, profile.ProcessName, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            configuration.Profiles.Add(profile);
            changed = true;
        }

        return changed;
    }

    private static bool IsNone(IReadOnlyDictionary<string, string> actionsByGesture, string gesture)
    {
        return actionsByGesture.TryGetValue(gesture, out var action)
            && string.Equals(action, "None", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAction(IReadOnlyDictionary<string, string> actionsByGesture, string gesture, string expectedAction)
    {
        return actionsByGesture.TryGetValue(gesture, out var action)
            && string.Equals(action, expectedAction, StringComparison.OrdinalIgnoreCase);
    }
}
