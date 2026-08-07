using MacBridgeWin.Core.Configuration;

namespace MacBridgeWin.App.ViewModels;

public sealed class MouseGestureActionViewModel
{
    public static IReadOnlyList<GestureActionOption> AvailableActions => GestureActionCatalog.Options;

    public bool Enabled { get; set; } = true;

    public string ModifierKey { get; set; } = GestureCatalog.RightButtonModifier;

    public string Gesture { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public bool ShowCursor { get; set; }

    public string Note { get; set; } = string.Empty;

    public static MouseGestureActionViewModel FromConfiguration(GestureActionConfiguration gesture)
    {
        return new MouseGestureActionViewModel
        {
            Enabled = gesture.Enabled,
            ModifierKey = gesture.ModifierKey,
            Gesture = gesture.Gesture,
            Action = gesture.Action,
            ShowCursor = gesture.ShowCursor,
            Note = gesture.Note
        };
    }

    public GestureActionConfiguration ToConfiguration()
    {
        return new GestureActionConfiguration(
            Gesture,
            Action,
            Enabled,
            ModifierKey,
            ShowCursor,
            Note);
    }
}
