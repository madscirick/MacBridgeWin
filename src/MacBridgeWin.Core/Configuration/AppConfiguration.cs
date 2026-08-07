namespace MacBridgeWin.Core.Configuration;

public sealed class AppConfiguration
{
    public int SchemaVersion { get; set; } = ConfigurationDefaults.CurrentSchemaVersion;

    public FeatureConfiguration Features { get; set; } = new();

    public StartupConfiguration Startup { get; set; } = new();

    public KeyboardConfiguration Keyboard { get; set; } = new();

    public MouseGestureConfiguration MouseGestures { get; set; } = new();

    public List<ApplicationProfileConfiguration> Profiles { get; set; } = [];
}
