using MacBridgeWin.Core.Keyboard;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MacBridgeWin.App.Services.Keyboard;

public sealed class GlobalHotkeyService : NativeWindow, IDisposable
{
    private const int WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint ModWin = 0x0008;
    private const uint ModNoRepeat = 0x4000;

    private readonly Action<KeyboardShortcut> _hotkeyPressed;
    private readonly Dictionary<int, KeyboardShortcut> _registeredHotkeys = [];
    private int _nextId = 100;

    public GlobalHotkeyService(Action<KeyboardShortcut> hotkeyPressed)
    {
        _hotkeyPressed = hotkeyPressed;
        CreateHandle(new CreateParams());
    }

    public int RegisterMany(IEnumerable<KeyboardShortcut> shortcuts)
    {
        var registeredCount = 0;

        foreach (var shortcut in shortcuts.Distinct())
        {
            if (!TryGetVirtualKey(shortcut.Key, out var virtualKey))
            {
                continue;
            }

            var id = _nextId++;
            if (!RegisterHotKey(Handle, id, ToNativeModifiers(shortcut.Modifiers), (uint)virtualKey))
            {
                continue;
            }

            _registeredHotkeys[id] = shortcut;
            registeredCount++;
        }

        return registeredCount;
    }

    public void Dispose()
    {
        foreach (var id in _registeredHotkeys.Keys)
        {
            UnregisterHotKey(Handle, id);
        }

        _registeredHotkeys.Clear();
        DestroyHandle();
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmHotkey && _registeredHotkeys.TryGetValue(m.WParam.ToInt32(), out var shortcut))
        {
            _hotkeyPressed(shortcut);
            return;
        }

        base.WndProc(ref m);
    }

    private static uint ToNativeModifiers(ShortcutModifiers modifiers)
    {
        var native = ModNoRepeat;

        if (modifiers.HasFlag(ShortcutModifiers.Alt))
        {
            native |= ModAlt;
        }

        if (modifiers.HasFlag(ShortcutModifiers.Ctrl))
        {
            native |= ModControl;
        }

        if (modifiers.HasFlag(ShortcutModifiers.Shift))
        {
            native |= ModShift;
        }

        if (modifiers.HasFlag(ShortcutModifiers.Win))
        {
            native |= ModWin;
        }

        return native;
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
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
