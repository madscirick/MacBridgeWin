namespace MacBridgeWin.Core.Configuration;

public static class GestureActionCatalog
{
    public const string None = "None";
    public const string CloseWindow = "KeyboardShortcut:Alt+F4";
    public const string BrowserBack = "KeyboardShortcut:Alt+Left";
    public const string BrowserForward = "KeyboardShortcut:Alt+Right";
    public const string NextTab = "KeyboardShortcut:Ctrl+Tab";
    public const string PreviousTab = "KeyboardShortcut:Ctrl+Shift+Tab";
    public const string NewTab = "KeyboardShortcut:Ctrl+T";
    public const string CloseTab = "KeyboardShortcut:Ctrl+W";
    public const string ReopenClosedTab = "KeyboardShortcut:Ctrl+Shift+T";
    public const string Refresh = "KeyboardShortcut:Ctrl+R";
    public const string Find = "KeyboardShortcut:Ctrl+F";
    public const string ScrollToTop = "KeyboardShortcut:Ctrl+Home";
    public const string ScrollToBottom = "KeyboardShortcut:Ctrl+End";
    public const string OpenTerminal = "LaunchApplication:wt.exe";
    public const string ExplorerUp = "KeyboardShortcut:Alt+Up";
    public const string OpenSelected = "KeyboardShortcut:Enter";
    public const string FocusSearch = "KeyboardShortcut:Ctrl+F";

    public static IReadOnlyList<GestureActionOption> Options { get; } =
    [
        new("None", None),
        new("Close Current Window", CloseWindow),
        new("Back", BrowserBack),
        new("Forward", BrowserForward),
        new("Next Tab", NextTab),
        new("Previous Tab", PreviousTab),
        new("New Tab", NewTab),
        new("Close Current Tab/Page", CloseTab),
        new("Reopen Closed Tab", ReopenClosedTab),
        new("Refresh", Refresh),
        new("Find", Find),
        new("Scroll to Top", ScrollToTop),
        new("Scroll to Bottom", ScrollToBottom),
        new("Open Windows Terminal", OpenTerminal),
        new("Explorer: Up One Level", ExplorerUp),
        new("Explorer: Open Selected", OpenSelected),
        new("Explorer: Focus Search", FocusSearch)
    ];

    public static string GetDisplayName(string action)
    {
        return Options.FirstOrDefault(option =>
            string.Equals(option.Action, action, StringComparison.OrdinalIgnoreCase))?.Name
            ?? action;
    }
}

public sealed record GestureActionOption(string Name, string Action);
