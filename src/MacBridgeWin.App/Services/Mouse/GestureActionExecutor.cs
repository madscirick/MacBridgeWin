using MacBridgeWin.App.Services.Keyboard;
using MacBridgeWin.Core.Configuration;
using MacBridgeWin.Core.Gestures;
using MacBridgeWin.Core.Keyboard;
using MacBridgeWin.Core.Logging;
using System.Diagnostics;

namespace MacBridgeWin.App.Services.Mouse;

public sealed class GestureActionExecutor(FileLogger logger)
{
    private readonly SyntheticKeyboardInputSender _keyboardInputSender = new();
    private readonly BrowserCommandInputSender _browserCommandInputSender = new();
    private List<GestureActionBinding> _actions = [];

    public void ApplyConfiguration(MouseGestureConfiguration configuration)
    {
        _actions = configuration.Gestures
            .Where(gesture => gesture.Enabled)
            .Where(gesture => GestureSequence.TryParse(gesture.Gesture, out _))
            .Select(gesture =>
            {
                GestureSequence.TryParse(gesture.Gesture, out var sequence);
                return new GestureActionBinding(sequence, gesture.Action);
            })
            .ToList();
    }

    public void Execute(IReadOnlyList<GestureDirection> sequence)
    {
        var binding = _actions.FirstOrDefault(action => GestureSequence.Equals(action.Sequence, sequence))
            ?? _actions.FirstOrDefault(action => GestureSequence.IsCompatible(action.Sequence, sequence));
        var gestureName = string.Join(",", sequence);

        if (binding is null || string.Equals(binding.Action, "None", StringComparison.OrdinalIgnoreCase))
        {
            logger.Info($"Mouse gesture '{gestureName}' recognized with no configured action.");
            return;
        }

        if (_browserCommandInputSender.TrySend(binding.Action))
        {
            logger.Info($"Mouse gesture '{gestureName}' executed browser command action '{binding.Action}'.");
            return;
        }

        if (TryLaunchApplication(binding.Action))
        {
            logger.Info($"Mouse gesture '{gestureName}' launched a configured application.");
            return;
        }

        if (TryParseKeyboardShortcutAction(binding.Action, out var shortcut))
        {
            _keyboardInputSender.SendShortcut(shortcut, ShortcutModifiers.None);
            logger.Info($"Mouse gesture '{gestureName}' executed keyboard shortcut action '{shortcut}'.");
            return;
        }

        logger.Info($"Mouse gesture '{gestureName}' recognized with an unsupported action type.");
    }

    private static bool TryLaunchApplication(string action)
    {
        const string prefix = "LaunchApplication:";
        var application = action.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? action[prefix.Length..].Trim()
            : string.Empty;
        if (application.Length == 0)
        {
            return false;
        }

        try
        {
            Process.Start(new ProcessStartInfo(application) { UseShellExecute = true });
            return true;
        }
        catch
        {
            if (!string.Equals(application, "wt.exe", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            try
            {
                Process.Start(new ProcessStartInfo("powershell.exe") { UseShellExecute = true });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    private static bool TryParseKeyboardShortcutAction(string action, out KeyboardShortcut shortcut)
    {
        shortcut = new KeyboardShortcut(ShortcutModifiers.None, string.Empty);

        const string longPrefix = "KeyboardShortcut:";
        const string shortPrefix = "Shortcut:";
        var shortcutText = action.StartsWith(longPrefix, StringComparison.OrdinalIgnoreCase)
            ? action[longPrefix.Length..]
            : action.StartsWith(shortPrefix, StringComparison.OrdinalIgnoreCase)
                ? action[shortPrefix.Length..]
                : string.Empty;

        return shortcutText.Length > 0 && KeyboardShortcut.TryParse(shortcutText, out shortcut);
    }

    private sealed record GestureActionBinding(IReadOnlyList<GestureDirection> Sequence, string Action);
}
