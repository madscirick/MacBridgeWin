using MacBridgeWin.App.ViewModels;
using MacBridgeWin.Core.Configuration;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MacBridgeWin.App;

public partial class SettingsWindow : Window
{
    private readonly IConfigurationStore _configurationStore;
    private readonly ConfigurationValidator _configurationValidator = new();
    private readonly SettingsViewModel _viewModel;
    private readonly Action<AppConfiguration>? _configurationSaved;
    private readonly DispatcherTimer _saveStatusTimer = new() { Interval = TimeSpan.FromSeconds(2) };

    public SettingsWindow(
        IConfigurationStore configurationStore,
        Action<AppConfiguration>? configurationSaved = null)
    {
        InitializeComponent();
        _configurationStore = configurationStore;
        _configurationSaved = configurationSaved;
        _viewModel = new SettingsViewModel(configurationStore.FilePath);
        DataContext = _viewModel;
        Loaded += SettingsWindow_Loaded;
        _saveStatusTimer.Tick += SaveStatusTimer_Tick;
    }

    private async void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var configuration = await _configurationStore.LoadAsync();
        _viewModel.Load(configuration);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        CommitPendingGridEdits();
        SaveButton.IsEnabled = false;
        SaveButton.Content = "Saving...";
        SaveStatusText.Visibility = Visibility.Collapsed;

        var configuration = await _configurationStore.LoadAsync();
        _viewModel.ApplyTo(configuration);
        var validation = _configurationValidator.Validate(configuration);
        if (!validation.IsValid)
        {
            ResetSaveButton();
            System.Windows.MessageBox.Show(
                string.Join(Environment.NewLine, validation.Errors),
                "Invalid settings",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        await _configurationStore.SaveAsync(configuration);
        _configurationSaved?.Invoke(configuration);
        ShowSavedStatus();
    }

    private async void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "JSON configuration (*.json)|*.json|All files (*.*)|*.*",
            Title = "Import configuration"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        var importedStore = new JsonConfigurationStore(dialog.FileName);
        var configuration = await importedStore.LoadAsync();
        var validation = _configurationValidator.Validate(configuration);
        if (!validation.IsValid)
        {
            System.Windows.MessageBox.Show(
                string.Join(Environment.NewLine, validation.Errors),
                "Invalid configuration",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        await _configurationStore.SaveAsync(configuration);
        _viewModel.Load(configuration);
        _configurationSaved?.Invoke(configuration);
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        CommitPendingGridEdits();
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JSON configuration (*.json)|*.json|All files (*.*)|*.*",
            FileName = "macbridgewin-config.json",
            Title = "Export configuration"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        var configuration = await _configurationStore.LoadAsync();
        _viewModel.ApplyTo(configuration);
        var validation = _configurationValidator.Validate(configuration);
        if (!validation.IsValid)
        {
            System.Windows.MessageBox.Show(
                string.Join(Environment.NewLine, validation.Errors),
                "Invalid settings",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var exportStore = new JsonConfigurationStore(dialog.FileName);
        await exportStore.SaveAsync(configuration);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void CommitPendingGridEdits()
    {
        CommitPendingGridEdit(KeyboardMappingsGrid);
        CommitPendingGridEdit(ProfileGestureActionsGrid);
    }

    private static void CommitPendingGridEdit(DataGrid grid)
    {
        grid.CommitEdit(DataGridEditingUnit.Cell, exitEditingMode: true);
        grid.CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true);
    }

    private void AddGestureScopeButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AppSelectionDialog
        {
            Owner = this
        };

        if (dialog.ShowDialog() != true || dialog.SelectedApplication is null)
        {
            return;
        }

        var selectedApplication = dialog.SelectedApplication;
        var existing = _viewModel.GestureScopes.FirstOrDefault(scope =>
            string.Equals(scope.ProcessName, selectedApplication.ProcessName, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            _viewModel.SelectedGestureScope = existing;
            GestureScopesList.ScrollIntoView(existing);
            return;
        }

        var scope = new GestureScopeViewModel
        {
            Name = selectedApplication.DisplayName,
            ProcessName = selectedApplication.ProcessName,
            MouseGestureThresholdPixels = _viewModel.GestureScopes.FirstOrDefault(scope => scope.IsGlobal)?.MouseGestureThresholdPixels ?? 48
        };
        scope.MouseGestures.Add(new MouseGestureActionViewModel
        {
            Enabled = true,
            ModifierKey = GestureCatalog.RightButtonModifier,
            Gesture = "Left",
            Action = "KeyboardShortcut:Alt+Left"
        });

        _viewModel.GestureScopes.Add(scope);
        _viewModel.SelectedGestureScope = scope;
    }

    private void RemoveGestureScopeButton_Click(object sender, RoutedEventArgs e)
    {
        var selected = _viewModel.SelectedGestureScope;
        if (selected is null || selected.IsGlobal)
        {
            return;
        }

        var selectedIndex = _viewModel.GestureScopes.IndexOf(selected);
        _viewModel.GestureScopes.Remove(selected);
        _viewModel.SelectedGestureScope = _viewModel.GestureScopes[Math.Max(0, selectedIndex - 1)];
    }

    private void AddProfileGestureButton_Click(object sender, RoutedEventArgs e)
    {
        var selected = _viewModel.SelectedGestureScope;
        if (selected is null)
        {
            return;
        }

        var gesture = new MouseGestureActionViewModel
        {
            Enabled = true,
            ModifierKey = GestureCatalog.RightButtonModifier,
            Gesture = "DownRight,UpRight",
            Action = "None"
        };
        selected.MouseGestures.Add(gesture);
        ProfileGestureActionsGrid.SelectedItem = gesture;
    }

    private void RemoveProfileGestureButton_Click(object sender, RoutedEventArgs e)
    {
        var selected = _viewModel.SelectedGestureScope;
        if (selected is null || ProfileGestureActionsGrid.SelectedItem is not MouseGestureActionViewModel gesture)
        {
            return;
        }

        selected.MouseGestures.Remove(gesture);
    }

    private void ShowSavedStatus()
    {
        SaveButton.Content = "Save";
        SaveButton.IsEnabled = true;
        SaveStatusText.Visibility = Visibility.Visible;
        _saveStatusTimer.Stop();
        _saveStatusTimer.Start();
    }

    private void ResetSaveButton()
    {
        SaveButton.Content = "Save";
        SaveButton.IsEnabled = true;
    }

    private void SaveStatusTimer_Tick(object? sender, EventArgs e)
    {
        _saveStatusTimer.Stop();
        SaveStatusText.Visibility = Visibility.Collapsed;
    }
}
