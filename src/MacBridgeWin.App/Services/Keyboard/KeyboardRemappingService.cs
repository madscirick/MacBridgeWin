using MacBridgeWin.Core.Configuration;
using MacBridgeWin.Core.Keyboard;
using MacBridgeWin.Core.Logging;
using MacBridgeWin.Core.Profiles;

namespace MacBridgeWin.App.Services.Keyboard;

public sealed class KeyboardRemappingService : IDisposable
{
    private readonly SyntheticKeyboardInputSender _inputSender = new();
    private readonly FileLogger _logger;
    private readonly ProfileResolver _profileResolver = new();
    private readonly Func<string?> _getActiveProcessName;
    private GlobalHotkeyService? _hotkeyService;
    private AppConfiguration _configuration = ConfigurationDefaults.Create();
    private bool _enabled;

    public KeyboardRemappingService(FileLogger logger, Func<string?> getActiveProcessName)
    {
        _logger = logger;
        _getActiveProcessName = getActiveProcessName;
    }

    public void ApplyConfiguration(AppConfiguration configuration)
    {
        _configuration = configuration;
        _enabled = configuration.Features.KeyboardMappingsEnabled;

        if (_enabled)
        {
            Start();
            return;
        }

        Stop();
    }

    public void Dispose()
    {
        Stop();
    }

    private void Start()
    {
        if (_hotkeyService is not null)
        {
            return;
        }

        _hotkeyService = new GlobalHotkeyService(HandleHotkey);
        var registeredCount = _hotkeyService.RegisterMany(GetRegisteredSourceShortcuts());
        _logger.Info($"Keyboard remapping enabled. Registered {registeredCount} global hotkeys.");
    }

    private void Stop()
    {
        if (_hotkeyService is null)
        {
            return;
        }

        _hotkeyService.Dispose();
        _hotkeyService = null;
        _logger.Info("Keyboard remapping disabled.");
    }

    private void HandleHotkey(KeyboardShortcut sourceShortcut)
    {
        var resolved = _profileResolver.Resolve(_configuration, _getActiveProcessName());
        var mapper = new KeyboardShortcutMapper(resolved.Keyboard.Mappings);
        var input = new KeyboardInput(sourceShortcut.Key, sourceShortcut.Modifiers, IsKeyDown: true, IsSynthetic: false);
        var decision = mapper.Decide(input, _enabled);
        if (decision.ShouldSuppressOriginal && decision.ReplacementShortcut is not null)
        {
            _inputSender.SendRemappedShortcut(sourceShortcut, decision.ReplacementShortcut);
            _logger.Info($"Keyboard hotkey remapped {sourceShortcut} to {decision.ReplacementShortcut}.");
        }
    }

    private IEnumerable<KeyboardShortcut> GetRegisteredSourceShortcuts()
    {
        foreach (var mapping in _configuration.Keyboard.Mappings)
        {
            if (KeyboardShortcut.TryParse(mapping.SourceShortcut, out var shortcut))
            {
                yield return shortcut;
            }
        }

        foreach (var profile in _configuration.Profiles)
        {
            foreach (var mapping in profile.Keyboard.Mappings)
            {
                if (KeyboardShortcut.TryParse(mapping.SourceShortcut, out var shortcut))
                {
                    yield return shortcut;
                }
            }
        }
    }
}
