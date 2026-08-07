using MacBridgeWin.Core.Configuration;
using MacBridgeWin.Core.Gestures;

namespace MacBridgeWin.Core.Profiles;

public sealed class ProfileResolver
{
    public ResolvedProfileConfiguration Resolve(AppConfiguration configuration, string? processName)
    {
        var profile = configuration.Profiles.FirstOrDefault(profile =>
            MatchesProcess(profile.ProcessName, processName));

        if (profile is null)
        {
            return new ResolvedProfileConfiguration(
                ProfileName: null,
                Keyboard: configuration.Keyboard,
                MouseGestures: configuration.MouseGestures);
        }

        return new ResolvedProfileConfiguration(
            ProfileName: profile.Name,
            Keyboard: profile.Keyboard,
            MouseGestures: MergeMouseGestures(configuration.MouseGestures, profile.MouseGestures));
    }

    private static MouseGestureConfiguration MergeMouseGestures(
        MouseGestureConfiguration globalGestures,
        MouseGestureConfiguration profileGestures)
    {
        var merged = new MouseGestureConfiguration
        {
            MovementThresholdPixels = profileGestures.MovementThresholdPixels > 0
                ? profileGestures.MovementThresholdPixels
                : globalGestures.MovementThresholdPixels,
            Gestures = globalGestures.Gestures.ToList()
        };

        foreach (var profileGesture in profileGestures.Gestures)
        {
            if (!GestureSequence.TryParse(profileGesture.Gesture, out var profileSequence))
            {
                continue;
            }

            var existingIndex = merged.Gestures.FindIndex(gesture =>
                GestureSequence.TryParse(gesture.Gesture, out var existingSequence)
                && GestureSequence.Equals(existingSequence, profileSequence));

            if (existingIndex >= 0)
            {
                merged.Gestures[existingIndex] = profileGesture;
                continue;
            }

            merged.Gestures.Add(profileGesture);
        }

        return merged;
    }

    private static bool MatchesProcess(string configuredProcessName, string? activeProcessName)
    {
        if (string.IsNullOrWhiteSpace(configuredProcessName) || string.IsNullOrWhiteSpace(activeProcessName))
        {
            return false;
        }

        return Normalize(configuredProcessName).Equals(Normalize(activeProcessName), StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string processName)
    {
        return Path.GetFileNameWithoutExtension(processName.Trim());
    }
}
