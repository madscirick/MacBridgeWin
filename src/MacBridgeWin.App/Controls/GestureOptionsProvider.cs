using MacBridgeWin.Core.Configuration;

namespace MacBridgeWin.App.Controls;

public static class GestureOptionsProvider
{
    public static IReadOnlyList<GestureModifierOption> ModifierOptions => GestureCatalog.ModifierOptions;

    public static IReadOnlyList<GestureOption> GestureOptions => GestureCatalog.GestureOptions;
}
