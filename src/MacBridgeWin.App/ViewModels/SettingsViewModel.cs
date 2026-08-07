using MacBridgeWin.Core.Configuration;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MacBridgeWin.App.ViewModels;

public sealed class SettingsViewModel(string configurationPath) : INotifyPropertyChanged
{
    private bool _keyboardMappingsEnabled;
    private bool _mouseGesturesEnabled;
    private bool _startWithWindows;
    private GestureScopeViewModel? _selectedGestureScope;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string ConfigurationPath { get; } = configurationPath;

    public ObservableCollection<KeyboardMappingViewModel> KeyboardMappings { get; } = [];

    public ObservableCollection<GestureScopeViewModel> GestureScopes { get; } = [];

    public GestureScopeViewModel? SelectedGestureScope
    {
        get => _selectedGestureScope;
        set => SetField(ref _selectedGestureScope, value);
    }

    public bool KeyboardMappingsEnabled
    {
        get => _keyboardMappingsEnabled;
        set => SetField(ref _keyboardMappingsEnabled, value);
    }

    public bool MouseGesturesEnabled
    {
        get => _mouseGesturesEnabled;
        set => SetField(ref _mouseGesturesEnabled, value);
    }

    public bool StartWithWindows
    {
        get => _startWithWindows;
        set => SetField(ref _startWithWindows, value);
    }

    public void Load(AppConfiguration configuration)
    {
        KeyboardMappingsEnabled = configuration.Features.KeyboardMappingsEnabled;
        MouseGesturesEnabled = configuration.Features.MouseGesturesEnabled;
        StartWithWindows = configuration.Startup.StartWithWindows;

        KeyboardMappings.Clear();
        foreach (var mapping in configuration.Keyboard.Mappings)
        {
            KeyboardMappings.Add(KeyboardMappingViewModel.FromConfiguration(mapping));
        }

        GestureScopes.Clear();
        GestureScopes.Add(GestureScopeViewModel.FromGlobal(configuration));
        foreach (var profile in configuration.Profiles)
        {
            GestureScopes.Add(GestureScopeViewModel.FromProfile(profile));
        }

        SelectedGestureScope = GestureScopes.FirstOrDefault();
    }

    public void ApplyTo(AppConfiguration configuration)
    {
        configuration.Features.KeyboardMappingsEnabled = KeyboardMappingsEnabled;
        configuration.Features.MouseGesturesEnabled = MouseGesturesEnabled;
        configuration.Startup.StartWithWindows = StartWithWindows;
        configuration.Keyboard.Mappings = KeyboardMappings
            .Where(mapping => !string.IsNullOrWhiteSpace(mapping.SourceShortcut)
                || !string.IsNullOrWhiteSpace(mapping.TargetShortcut))
            .Select(mapping => mapping.ToConfiguration())
            .ToList();
        var globalScope = GestureScopes.FirstOrDefault(scope => scope.IsGlobal);
        globalScope?.ApplyToGlobal(configuration);
        configuration.Profiles = GestureScopes
            .Where(scope => !scope.IsGlobal)
            .Where(scope => !string.IsNullOrWhiteSpace(scope.Name)
                || !string.IsNullOrWhiteSpace(scope.ProcessName))
            .Select(scope => scope.ToProfileConfiguration())
            .ToList();
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
