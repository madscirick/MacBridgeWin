using MacBridgeWin.Core.Keyboard;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MacBridgeWin.App.Services.Keyboard;

public sealed class KeyboardHookService : IDisposable
{
    private const int WhKeyboardLl = 13;
    private const int WmKeyDown = 0x0100;
    private const int WmSysKeyDown = 0x0104;
    private const int LlkhfInjected = 0x00000010;
    private const int VkShift = 0x10;
    private const int VkControl = 0x11;
    private const int VkMenu = 0x12;
    private const int VkLWin = 0x5B;
    private const int VkRWin = 0x5C;

    private readonly LowLevelKeyboardProc _hookProc;
    private readonly Func<KeyboardInput, KeyboardRemapDecision> _decide;
    private readonly Action<KeyboardShortcut, ShortcutModifiers> _sendReplacement;
    private IntPtr _hookHandle;

    public KeyboardHookService(
        Func<KeyboardInput, KeyboardRemapDecision> decide,
        Action<KeyboardShortcut, ShortcutModifiers> sendReplacement)
    {
        _decide = decide;
        _sendReplacement = sendReplacement;
        _hookProc = HookCallback;
    }

    public void Start()
    {
        if (_hookHandle != IntPtr.Zero)
        {
            return;
        }

        using var currentProcess = Process.GetCurrentProcess();
        using var currentModule = currentProcess.MainModule;
        var moduleHandle = currentModule is null ? IntPtr.Zero : GetModuleHandle(currentModule.ModuleName);
        _hookHandle = SetWindowsHookEx(WhKeyboardLl, _hookProc, moduleHandle, 0);

        if (_hookHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to install the low-level keyboard hook.");
        }
    }

    public void Stop()
    {
        if (_hookHandle == IntPtr.Zero)
        {
            return;
        }

        UnhookWindowsHookEx(_hookHandle);
        _hookHandle = IntPtr.Zero;
    }

    public void Dispose()
    {
        Stop();
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode < 0)
        {
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        var message = wParam.ToInt32();
        if (message is not WmKeyDown and not WmSysKeyDown)
        {
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        var hookInfo = Marshal.PtrToStructure<KbdLlHookStruct>(lParam);
        var key = NormalizeKeyName((Keys)hookInfo.VkCode);
        if (key.Length == 0)
        {
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        var modifiers = GetCurrentModifiers();
        var isSynthetic = (hookInfo.Flags & LlkhfInjected) != 0
            && hookInfo.DwExtraInfo == SyntheticKeyboardInputSender.ExtraInfoMarker;
        var input = new KeyboardInput(key, modifiers, IsKeyDown: true, isSynthetic);
        var decision = _decide(input);

        if (!decision.ShouldSuppressOriginal || decision.ReplacementShortcut is null)
        {
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        _sendReplacement(decision.ReplacementShortcut, modifiers);
        return new IntPtr(1);
    }

    private static ShortcutModifiers GetCurrentModifiers()
    {
        var modifiers = ShortcutModifiers.None;

        if (IsKeyPressed(VkShift))
        {
            modifiers |= ShortcutModifiers.Shift;
        }

        if (IsKeyPressed(VkControl))
        {
            modifiers |= ShortcutModifiers.Ctrl;
        }

        if (IsKeyPressed(VkMenu))
        {
            modifiers |= ShortcutModifiers.Alt;
        }

        if (IsKeyPressed(VkLWin) || IsKeyPressed(VkRWin))
        {
            modifiers |= ShortcutModifiers.Win;
        }

        return modifiers;
    }

    private static bool IsKeyPressed(int virtualKey)
    {
        return (GetAsyncKeyState(virtualKey) & 0x8000) != 0;
    }

    private static string NormalizeKeyName(Keys key)
    {
        if (key is >= Keys.A and <= Keys.Z)
        {
            return key.ToString().ToUpperInvariant();
        }

        if (key is >= Keys.F1 and <= Keys.F24)
        {
            return key.ToString().ToUpperInvariant();
        }

        return key switch
        {
            Keys.Tab => "TAB",
            _ => string.Empty
        };
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct KbdLlHookStruct
    {
        public readonly uint VkCode;
        public readonly uint ScanCode;
        public readonly uint Flags;
        public readonly uint Time;
        public readonly UIntPtr DwExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
}
