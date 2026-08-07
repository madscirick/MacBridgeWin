using MacBridgeWin.Core.Configuration;

namespace MacBridgeWin.App.ViewModels;

public sealed class ApplicationProfileViewModel
{
    private KeyboardConfiguration _keyboard = ConfigurationDefaults.Create().Keyboard;

    public string Name { get; set; } = string.Empty;

    public string ProcessName { get; set; } = string.Empty;

    public int MouseGestureThresholdPixels { get; set; } = 48;

    public string MouseGestureActions { get; set; } = string.Empty;

    public static ApplicationProfileViewModel FromConfiguration(ApplicationProfileConfiguration profile)
    {
        return new ApplicationProfileViewModel
        {
            Name = profile.Name,
            ProcessName = profile.ProcessName,
            _keyboard = profile.Keyboard,
            MouseGestureThresholdPixels = profile.MouseGestures.MovementThresholdPixels,
            MouseGestureActions = FormatMouseGestureActions(profile.MouseGestures.Gestures)
        };
    }

    public ApplicationProfileConfiguration ToConfiguration()
    {
        return new ApplicationProfileConfiguration
        {
            Name = Name,
            ProcessName = ProcessName,
            Keyboard = _keyboard,
            MouseGestures = new MouseGestureConfiguration
            {
                MovementThresholdPixels = MouseGestureThresholdPixels,
                Gestures = ParseMouseGestureActions(MouseGestureActions)
            }
        };
    }

    private static string FormatMouseGestureActions(IEnumerable<GestureActionConfiguration> gestures)
    {
        return string.Join(
            Environment.NewLine,
            gestures.Select(gesture => $"{gesture.Gesture}={gesture.Action}"));
    }

    private static List<GestureActionConfiguration> ParseMouseGestureActions(string value)
    {
        return value
            .Split([Environment.NewLine, "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ParseMouseGestureAction)
            .Where(gesture => gesture is not null)
            .Cast<GestureActionConfiguration>()
            .ToList();
    }

    private static GestureActionConfiguration? ParseMouseGestureAction(string line)
    {
        var separatorIndex = line.IndexOf('=');
        if (separatorIndex < 0)
        {
            separatorIndex = line.IndexOf(':');
        }

        if (separatorIndex <= 0 || separatorIndex >= line.Length - 1)
        {
            return null;
        }

        var gesture = line[..separatorIndex].Trim();
        var action = line[(separatorIndex + 1)..].Trim();
        return new GestureActionConfiguration(gesture, action);
    }
}
