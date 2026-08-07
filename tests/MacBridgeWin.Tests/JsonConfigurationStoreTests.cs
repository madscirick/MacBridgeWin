using MacBridgeWin.Core.Configuration;

namespace MacBridgeWin.Tests;

[TestClass]
public sealed class JsonConfigurationStoreTests
{
    [TestMethod]
    public async Task LoadAsync_CreatesDefaultConfiguration_WhenFileDoesNotExist()
    {
        var directory = CreateTemporaryDirectory();
        var filePath = Path.Combine(directory, "config.json");
        var store = new JsonConfigurationStore(filePath);

        var configuration = await store.LoadAsync();

        Assert.IsTrue(File.Exists(filePath));
        Assert.AreEqual(ConfigurationDefaults.CurrentSchemaVersion, configuration.SchemaVersion);
        Assert.IsTrue(configuration.Keyboard.Mappings.Count > 0);
    }

    [TestMethod]
    public async Task SaveAsync_PersistsFeatureToggles()
    {
        var directory = CreateTemporaryDirectory();
        var filePath = Path.Combine(directory, "config.json");
        var store = new JsonConfigurationStore(filePath);
        var configuration = ConfigurationDefaults.Create();
        configuration.Features.KeyboardMappingsEnabled = true;
        configuration.Features.MouseGesturesEnabled = true;
        configuration.Startup.StartWithWindows = true;

        await store.SaveAsync(configuration);
        var loaded = await store.LoadAsync();

        Assert.IsTrue(loaded.Features.KeyboardMappingsEnabled);
        Assert.IsTrue(loaded.Features.MouseGesturesEnabled);
        Assert.IsTrue(loaded.Startup.StartWithWindows);
    }

    private static string CreateTemporaryDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "MacBridgeWin.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }
}
