using MacBridgeWin.Core.Configuration;
using MacBridgeWin.Core.Keyboard;

namespace MacBridgeWin.Tests;

[TestClass]
public sealed class KeyboardShortcutMapperTests
{
    [TestMethod]
    public void Decide_RemapsAltC_ToCtrlC()
    {
        var mapper = CreateDefaultMapper();
        var input = new KeyboardInput("C", ShortcutModifiers.Alt, IsKeyDown: true, IsSynthetic: false);

        var decision = mapper.Decide(input, mappingsEnabled: true);

        Assert.IsTrue(decision.ShouldSuppressOriginal);
        Assert.AreEqual(ShortcutModifiers.Ctrl, decision.ReplacementShortcut?.Modifiers);
        Assert.AreEqual("C", decision.ReplacementShortcut?.Key);
    }

    [TestMethod]
    public void Decide_RemapsAltShiftZ_ToCtrlY()
    {
        var mapper = CreateDefaultMapper();
        var input = new KeyboardInput("Z", ShortcutModifiers.Alt | ShortcutModifiers.Shift, IsKeyDown: true, IsSynthetic: false);

        var decision = mapper.Decide(input, mappingsEnabled: true);

        Assert.IsTrue(decision.ShouldSuppressOriginal);
        Assert.AreEqual(ShortcutModifiers.Ctrl, decision.ReplacementShortcut?.Modifiers);
        Assert.AreEqual("Y", decision.ReplacementShortcut?.Key);
    }

    [TestMethod]
    public void Decide_PreservesNativeCtrlShortcuts()
    {
        var mapper = CreateDefaultMapper();
        var input = new KeyboardInput("C", ShortcutModifiers.Ctrl, IsKeyDown: true, IsSynthetic: false);

        var decision = mapper.Decide(input, mappingsEnabled: true);

        Assert.IsFalse(decision.ShouldSuppressOriginal);
    }

    [TestMethod]
    public void Decide_PreservesAltTabAndAltF4()
    {
        var mapper = CreateDefaultMapper();

        var altTab = mapper.Decide(new KeyboardInput("Tab", ShortcutModifiers.Alt, IsKeyDown: true, IsSynthetic: false), true);
        var altF4 = mapper.Decide(new KeyboardInput("F4", ShortcutModifiers.Alt, IsKeyDown: true, IsSynthetic: false), true);

        Assert.IsFalse(altTab.ShouldSuppressOriginal);
        Assert.IsFalse(altF4.ShouldSuppressOriginal);
    }

    [TestMethod]
    public void Decide_IgnoresSyntheticInput()
    {
        var mapper = CreateDefaultMapper();
        var input = new KeyboardInput("C", ShortcutModifiers.Alt, IsKeyDown: true, IsSynthetic: true);

        var decision = mapper.Decide(input, mappingsEnabled: true);

        Assert.IsFalse(decision.ShouldSuppressOriginal);
    }

    [TestMethod]
    public void Decide_DoesNothing_WhenMappingsAreDisabled()
    {
        var mapper = CreateDefaultMapper();
        var input = new KeyboardInput("C", ShortcutModifiers.Alt, IsKeyDown: true, IsSynthetic: false);

        var decision = mapper.Decide(input, mappingsEnabled: false);

        Assert.IsFalse(decision.ShouldSuppressOriginal);
    }

    private static KeyboardShortcutMapper CreateDefaultMapper()
    {
        return new KeyboardShortcutMapper(ConfigurationDefaults.Create().Keyboard.Mappings);
    }
}
