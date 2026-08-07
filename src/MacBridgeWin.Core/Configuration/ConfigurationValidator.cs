using MacBridgeWin.Core.Gestures;
using MacBridgeWin.Core.Keyboard;

namespace MacBridgeWin.Core.Configuration;

public sealed class ConfigurationValidator
{
    public ConfigurationValidationResult Validate(AppConfiguration configuration)
    {
        var result = new ConfigurationValidationResult();

        ValidateKeyboardMappings(configuration, result);
        ValidateMouseGestures(configuration, result);

        return result;
    }

    private static void ValidateKeyboardMappings(AppConfiguration configuration, ConfigurationValidationResult result)
    {
        for (var index = 0; index < configuration.Keyboard.Mappings.Count; index++)
        {
            var mapping = configuration.Keyboard.Mappings[index];
            if (!KeyboardShortcut.TryParse(mapping.SourceShortcut, out _))
            {
                result.AddError($"Keyboard mapping {index + 1} has an invalid source shortcut.");
            }

            if (!KeyboardShortcut.TryParse(mapping.TargetShortcut, out _))
            {
                result.AddError($"Keyboard mapping {index + 1} has an invalid target shortcut.");
            }
        }
    }

    private static void ValidateMouseGestures(AppConfiguration configuration, ConfigurationValidationResult result)
    {
        if (configuration.MouseGestures.MovementThresholdPixels is < 8 or > 512)
        {
            result.AddError("Mouse gesture threshold must be between 8 and 512 pixels.");
        }

        for (var index = 0; index < configuration.MouseGestures.Gestures.Count; index++)
        {
            var gesture = configuration.MouseGestures.Gestures[index];
            if (!GestureSequence.TryParse(gesture.Gesture, out _))
            {
                result.AddError($"Mouse gesture {index + 1} has an invalid direction.");
            }

            if (!IsValidGestureAction(gesture.Action))
            {
                result.AddError($"Mouse gesture {index + 1} has an invalid action.");
            }
        }

        for (var index = 0; index < configuration.Profiles.Count; index++)
        {
            var profile = configuration.Profiles[index];
            if (string.IsNullOrWhiteSpace(profile.Name))
            {
                result.AddError($"Profile {index + 1} has an empty name.");
            }

            if (string.IsNullOrWhiteSpace(profile.ProcessName))
            {
                result.AddError($"Profile {index + 1} has an empty process name.");
            }

            ValidateKeyboardMappings(new AppConfiguration { Keyboard = profile.Keyboard }, result);
            ValidateMouseGestures(new AppConfiguration { MouseGestures = profile.MouseGestures }, result);
        }
    }

    private static bool IsValidGestureAction(string action)
    {
        if (string.Equals(action, "None", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        const string launchPrefix = "LaunchApplication:";
        if (action.StartsWith(launchPrefix, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(action[launchPrefix.Length..]))
        {
            return true;
        }

        const string longPrefix = "KeyboardShortcut:";
        const string shortPrefix = "Shortcut:";
        var shortcutText = action.StartsWith(longPrefix, StringComparison.OrdinalIgnoreCase)
            ? action[longPrefix.Length..]
            : action.StartsWith(shortPrefix, StringComparison.OrdinalIgnoreCase)
                ? action[shortPrefix.Length..]
                : string.Empty;

        return shortcutText.Length > 0 && KeyboardShortcut.TryParse(shortcutText, out _);
    }
}
