using MacBridgeWin.Core.Configuration;

namespace MacBridgeWin.Tests;

[TestClass]
public sealed class ConfigurationValidatorTests
{
    private readonly ConfigurationValidator _validator = new();

    [TestMethod]
    public void Validate_AcceptsDefaultConfiguration()
    {
        var result = _validator.Validate(ConfigurationDefaults.Create());

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_RejectsInvalidKeyboardShortcut()
    {
        var configuration = ConfigurationDefaults.Create();
        configuration.Keyboard.Mappings[0] = new KeyboardMappingConfiguration("Alt+C+V", "Ctrl+C");

        var result = _validator.Validate(configuration);

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Validate_RejectsInvalidGestureDirection()
    {
        var configuration = ConfigurationDefaults.Create();
        configuration.MouseGestures.Gestures[0] = new GestureActionConfiguration("Circle", "None");

        var result = _validator.Validate(configuration);

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Validate_AcceptsKeyboardShortcutGestureAction()
    {
        var configuration = ConfigurationDefaults.Create();
        configuration.MouseGestures.Gestures[0] = new GestureActionConfiguration("Left,Up", "KeyboardShortcut:Ctrl+W");

        var result = _validator.Validate(configuration);

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_RejectsProfileWithoutProcessName()
    {
        var configuration = ConfigurationDefaults.Create();
        configuration.Profiles.Add(new ApplicationProfileConfiguration
        {
            Name = "Missing process",
            ProcessName = string.Empty,
            Keyboard = ConfigurationDefaults.Create().Keyboard,
            MouseGestures = ConfigurationDefaults.Create().MouseGestures
        });

        var result = _validator.Validate(configuration);

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Validate_RejectsOutOfRangeGestureThreshold()
    {
        var configuration = ConfigurationDefaults.Create();
        configuration.MouseGestures.MovementThresholdPixels = 2;

        var result = _validator.Validate(configuration);

        Assert.IsFalse(result.IsValid);
    }
}
