using System.Text.Json;

namespace MacBridgeWin.Core.Configuration;

public sealed class JsonConfigurationStore(string filePath) : IConfigurationStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public string FilePath { get; } = filePath;

    public async Task<AppConfiguration> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(FilePath))
        {
            var defaults = ConfigurationDefaults.Create();
            await SaveAsync(defaults, cancellationToken).ConfigureAwait(false);
            return defaults;
        }

        AppConfiguration? configuration;
        await using (var stream = File.OpenRead(FilePath))
        {
            configuration = await JsonSerializer.DeserializeAsync<AppConfiguration>(
                stream,
                SerializerOptions,
                cancellationToken).ConfigureAwait(false);
        }

        configuration ??= ConfigurationDefaults.Create();
        if (configuration.SchemaVersion < ConfigurationDefaults.CurrentSchemaVersion)
        {
            ConfigurationDefaults.ApplyRecommendedMouseGestureActions(configuration);
            configuration.SchemaVersion = ConfigurationDefaults.CurrentSchemaVersion;
            await SaveAsync(configuration, cancellationToken).ConfigureAwait(false);
        }
        return configuration;
    }

    public async Task SaveAsync(AppConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(FilePath);
        await JsonSerializer.SerializeAsync(
            stream,
            configuration,
            SerializerOptions,
            cancellationToken).ConfigureAwait(false);
    }
}
