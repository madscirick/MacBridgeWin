using MacBridgeWin.Core.Keyboard;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MacBridgeWin.App.Services.Keyboard;

public sealed class SyntheticKeyboardInputSender
{
    public static readonly UIntPtr ExtraInfoMarker = new(0x4D42574B);

    private const uint InputKeyboard = 1;
    private const uint KeyEventFKeyUp = 0x0002;
    private const uint KeyEventFExtendedKey = 0x0001;
    private const int KeyPressed = 0x8000;

    public void SendShortcut(KeyboardShortcut shortcut, ShortcutModifiers currentlyPressedModifiers)
    {
        if (!TryGetVirtualKey(shortcut.Key, out var key))
        {
            return;
        }

        if (currentlyPressedModifiers == ShortcutModifiers.None
            && TryBuildSendKeysText(shortcut, out var sendKeysText)
            && System.Windows.Application.Current is { } application)
        {
            try
            {
                application.Dispatcher.Invoke(() => SendKeys.SendWait(sendKeysText));
                return;
            }
            catch (InvalidOperationException)
            {
                // Fall back to SendInput when the WPF dispatcher is unavailable.
            }
        }

        var inputs = new List<Input>();
        var modifiersToRelease = currentlyPressedModifiers & ~shortcut.Modifiers;
        AddModifierEvents(inputs, modifiersToRelease, keyDown: false);
        AddModifierEvents(inputs, shortcut.Modifiers & ~currentlyPressedModifiers, keyDown: true);
        AddKeyEvent(inputs, key, keyDown: true);
        AddKeyEvent(inputs, key, keyDown: false);
        AddModifierEvents(inputs, shortcut.Modifiers & ~currentlyPressedModifiers, keyDown: false);
        AddModifierEvents(inputs, modifiersToRelease, keyDown: true);

        if (inputs.Count == 0)
        {
            return;
        }

        SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf<Input>());
    }

    public void SendKey(Keys key)
    {
        var inputs = new List<Input>();
        AddKeyEvent(inputs, key, keyDown: true);
        AddKeyEvent(inputs, key, keyDown: false);
        SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf<Input>());
    }

    public async void SendRemappedShortcut(KeyboardShortcut sourceShortcut, KeyboardShortcut targetShortcut)
    {
        if (!TryGetVirtualKey(sourceShortcut.Key, out var sourceKey)
            || !TryGetVirtualKey(targetShortcut.Key, out var targetKey))
        {
            return;
        }

        await WaitForSourceShortcutReleaseAsync(sourceKey, sourceShortcut.Modifiers);
        await Task.Delay(50);

        if (TryBuildSendKeysText(targetShortcut, out var sendKeysText))
        {
            SendKeys.SendWait(sendKeysText);
            return;
        }

        var inputs = new List<Input>();
        AddModifierEvents(inputs, targetShortcut.Modifiers, keyDown: true);
        AddKeyEvent(inputs, targetKey, keyDown: true);
        AddKeyEvent(inputs, targetKey, keyDown: false);
        AddModifierEvents(inputs, targetShortcut.Modifiers, keyDown: false);

        SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf<Input>());
    }

    private static bool TryBuildSendKeysText(KeyboardShortcut shortcut, out string sendKeysText)
    {
        sendKeysText = string.Empty;

        if (shortcut.Key.Length != 1
            && !shortcut.Key.StartsWith("F", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(shortcut.Key, "Tab", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var prefix = string.Empty;
        if (shortcut.Modifiers.HasFlag(ShortcutModifiers.Ctrl))
        {
            prefix += "^";
        }

        if (shortcut.Modifiers.HasFlag(ShortcutModifiers.Alt))
        {
            prefix += "%";
        }

        if (shortcut.Modifiers.HasFlag(ShortcutModifiers.Shift))
        {
            prefix += "+";
        }

        var keyText = shortcut.Key.Length == 1
            ? shortcut.Key.ToLowerInvariant()
            : $"{{{shortcut.Key.ToUpperInvariant()}}}";

        sendKeysText = prefix + keyText;
        return true;
    }

    private static async Task WaitForSourceShortcutReleaseAsync(Keys sourceKey, ShortcutModifiers sourceModifiers)
    {
        for (var attempt = 0; attempt < 30; attempt++)
        {
            if (!IsKeyPressed(sourceKey) && !AreModifiersPressed(sourceModifiers))
            {
                return;
            }

            await Task.Delay(10).ConfigureAwait(false);
        }
    }

    private static bool AreModifiersPressed(ShortcutModifiers modifiers)
    {
        return modifiers.HasFlag(ShortcutModifiers.Alt) && IsKeyPressed(Keys.Menu)
            || modifiers.HasFlag(ShortcutModifiers.Ctrl) && IsKeyPressed(Keys.ControlKey)
            || modifiers.HasFlag(ShortcutModifiers.Shift) && IsKeyPressed(Keys.ShiftKey)
            || modifiers.HasFlag(ShortcutModifiers.Win) && (IsKeyPressed(Keys.LWin) || IsKeyPressed(Keys.RWin));
    }

    private static bool IsKeyPressed(Keys key)
    {
        return (GetAsyncKeyState((int)key) & KeyPressed) != 0;
    }

    private static void AddModifierEvents(ICollection<Input> inputs, ShortcutModifiers modifiers, bool keyDown)
    {
        if (modifiers.HasFlag(ShortcutModifiers.Win))
        {
            AddKeyEvent(inputs, Keys.LWin, keyDown);
        }

        if (modifiers.HasFlag(ShortcutModifiers.Shift))
        {
            AddKeyEvent(inputs, Keys.ShiftKey, keyDown);
        }

        if (modifiers.HasFlag(ShortcutModifiers.Alt))
        {
            AddKeyEvent(inputs, Keys.Menu, keyDown);
        }

        if (modifiers.HasFlag(ShortcutModifiers.Ctrl))
        {
            AddKeyEvent(inputs, Keys.ControlKey, keyDown);
        }
    }

    private static void AddKeyEvent(ICollection<Input> inputs, Keys key, bool keyDown)
    {
        var flags = keyDown ? 0 : KeyEventFKeyUp;
        if (IsExtendedKey(key))
        {
            flags |= KeyEventFExtendedKey;
        }

        inputs.Add(new Input
        {
            Type = InputKeyboard,
            Data = new InputUnion
            {
                Keyboard = new KeyboardInput
                {
                    VirtualKey = (ushort)key,
                    ScanCode = 0,
                    Flags = flags,
                    Time = 0,
                    ExtraInfo = ExtraInfoMarker
                }
            }
        });
    }

    private static bool IsExtendedKey(Keys key)
    {
        return key is Keys.Left
            or Keys.Right
            or Keys.Up
            or Keys.Down
            or Keys.Home
            or Keys.End
            or Keys.Prior
            or Keys.Next
            or Keys.Insert
            or Keys.Delete
            or Keys.Divide
            or Keys.NumLock
            or Keys.RControlKey
            or Keys.RMenu;
    }

    private static bool TryGetVirtualKey(string key, out Keys virtualKey)
    {
        if (key.Length == 1 && key[0] is >= 'A' and <= 'Z')
        {
            virtualKey = (Keys)key[0];
            return true;
        }

        if (Enum.TryParse(key, ignoreCase: true, out Keys parsed))
        {
            virtualKey = parsed;
            return true;
        }

        virtualKey = Keys.None;
        return false;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint Type;
        public InputUnion Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public KeyboardInput Keyboard;

        // INPUT contains a union of MOUSEINPUT, KEYBDINPUT and HARDWAREINPUT.
        // Including the largest member is required so Marshal.SizeOf<Input>()
        // matches Win32's INPUT size (40 bytes on x64, 28 bytes on x86).
        [FieldOffset(0)]
        public MouseInput Mouse;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInput
    {
        public ushort VirtualKey;
        public ushort ScanCode;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }
}
