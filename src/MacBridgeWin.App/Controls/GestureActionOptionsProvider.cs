using MacBridgeWin.Core.Configuration;

namespace MacBridgeWin.App.Controls;

public static class GestureActionOptionsProvider
{
    public static IReadOnlyList<GestureActionOption> Options => GestureActionCatalog.Options;
}
