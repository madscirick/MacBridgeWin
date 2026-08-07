namespace MacBridgeWin.Core.Configuration;

public interface IConfigurationStore
{
    string FilePath { get; }

    Task<AppConfiguration> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(AppConfiguration configuration, CancellationToken cancellationToken = default);
}
