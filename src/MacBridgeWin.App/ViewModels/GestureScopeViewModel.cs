using MacBridgeWin.Core.Configuration;
using System.Collections.ObjectModel;

namespace MacBridgeWin.App.ViewModels;

public sealed class GestureScopeViewModel
{
    private KeyboardConfiguration _keyboard = ConfigurationDefaults.Create().Keyboard;

    public bool IsGlobal { get; init; }

    public string Name { get; set; } = string.Empty;

    public string ProcessName { get; set; } = string.Empty;

    public int MouseGestureThresholdPixels { get; set; } = 48;

    public ObservableCollection<MouseGestureActionViewModel> MouseGestures { get; } = [];

    public string DisplayName => IsGlobal ? "All applications" : Name;

    public string DisplayProcess => IsGlobal ? "-" : ProcessName;

    public static GestureScopeViewModel FromGlobal(AppConfiguration configuration)
    {
        var scope = new GestureScopeViewModel
        {
            IsGlobal = true,
            Name = "All applications",
            MouseGestureThresholdPixels = configuration.MouseGestures.MovementThresholdPixels
        };

        foreach (var gesture in configuration.MouseGestures.Gestures)
        {
            scope.MouseGestures.Add(MouseGestureActionViewModel.FromConfiguration(gesture));
        }

        return scope;
    }

    public static GestureScopeViewModel FromProfile(ApplicationProfileConfiguration profile)
    {
        var scope = new GestureScopeViewModel
        {
            Name = profile.Name,
            ProcessName = profile.ProcessName,
            _keyboard = profile.Keyboard,
            MouseGestureThresholdPixels = profile.MouseGestures.MovementThresholdPixels
        };

        foreach (var gesture in profile.MouseGestures.Gestures)
        {
            scope.MouseGestures.Add(MouseGestureActionViewModel.FromConfiguration(gesture));
        }

        return scope;
    }

    public void ApplyToGlobal(AppConfiguration configuration)
    {
        configuration.MouseGestures.MovementThresholdPixels = MouseGestureThresholdPixels;
        configuration.MouseGestures.Gestures = MouseGestures
            .Where(gesture => !string.IsNullOrWhiteSpace(gesture.Gesture)
                || !string.IsNullOrWhiteSpace(gesture.Action))
            .Select(gesture => gesture.ToConfiguration())
            .ToList();
    }

    public ApplicationProfileConfiguration ToProfileConfiguration()
    {
        return new ApplicationProfileConfiguration
        {
            Name = Name,
            ProcessName = ProcessName,
            Keyboard = _keyboard,
            MouseGestures = new MouseGestureConfiguration
            {
                MovementThresholdPixels = MouseGestureThresholdPixels,
                Gestures = MouseGestures
                    .Where(gesture => !string.IsNullOrWhiteSpace(gesture.Gesture)
                        || !string.IsNullOrWhiteSpace(gesture.Action))
                    .Select(gesture => gesture.ToConfiguration())
                    .ToList()
            }
        };
    }
}
