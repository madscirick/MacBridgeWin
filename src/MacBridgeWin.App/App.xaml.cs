using MacBridgeWin.App.Services;
using MacBridgeWin.App.Services.Keyboard;
using MacBridgeWin.App.Services.Mouse;
using MacBridgeWin.App.Services.Profiles;
using MacBridgeWin.App.Services.Startup;
using MacBridgeWin.Core.Configuration;
using MacBridgeWin.Core.Logging;
using System.Windows.Threading;
using System.Windows;

namespace MacBridgeWin.App;

public partial class App : System.Windows.Application
{
    private const string SingleInstanceMutexName = "MacBridgeWin.SingleInstance";
    private const string ShowSettingsEventName = "MacBridgeWin.ShowSettings";

    private Mutex? _singleInstanceMutex;
    private EventWaitHandle? _showSettingsEvent;
    private CancellationTokenSource? _showSettingsListenerCancellation;
    private TrayIconService? _trayIconService;
    private SettingsWindow? _settingsWindow;
    private JsonConfigurationStore? _configurationStore;
    private KeyboardRemappingService? _keyboardRemappingService;
    private MouseGestureService? _mouseGestureService;
    private ActiveApplicationProvider? _activeApplicationProvider;
    private WindowsStartupService? _windowsStartupService;
    private FileLogger? _logger;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var showSettingsRequested = e.Args.Any(argument =>
            string.Equals(argument, "--show-settings", StringComparison.OrdinalIgnoreCase));
        _singleInstanceMutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out var ownsMutex);
        if (!ownsMutex)
        {
            if (showSettingsRequested)
            {
                RequestSettingsWindow();
            }

            Shutdown();
            return;
        }

        _showSettingsEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ShowSettingsEventName);
        _showSettingsListenerCancellation = new CancellationTokenSource();
        _ = Task.Run(() => ListenForSettingsRequests(_showSettingsEvent, _showSettingsListenerCancellation.Token));

        _logger = FileLogger.CreateDefault("MacBridgeWin");
        _logger.Info("MacBridgeWin starting.");
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        var configurationPath = AppPaths.GetConfigurationPath();
        _configurationStore = new JsonConfigurationStore(configurationPath);

        try
        {
            var configuration = await _configurationStore.LoadAsync();
            _activeApplicationProvider = new ActiveApplicationProvider();
            _windowsStartupService = new WindowsStartupService();
            _windowsStartupService.Apply(configuration.Startup.StartWithWindows);
            _keyboardRemappingService = new KeyboardRemappingService(_logger, _activeApplicationProvider.GetActiveProcessName);
            _keyboardRemappingService.ApplyConfiguration(configuration);
            _mouseGestureService = new MouseGestureService(
                _logger,
                _activeApplicationProvider.GetActiveProcessName,
                _activeApplicationProvider.GetProcessNameFromPoint,
                new MouseGestureFeedbackService(this));
            _mouseGestureService.ApplyConfiguration(configuration);
        }
        catch (Exception ex)
        {
            _logger.Error("Configuration failed to load. Defaults will be used.", ex);
        }

        _trayIconService = new TrayIconService(ShowSettingsWindow, ExitApplication);
        _trayIconService.Show();
        _logger.Info("MacBridgeWin is running in the notification area.");

        if (showSettingsRequested)
        {
            _ = Dispatcher.BeginInvoke(ShowSettingsWindow);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _logger?.Info("MacBridgeWin exiting.");
        _showSettingsListenerCancellation?.Cancel();
        _showSettingsEvent?.Dispose();
        _showSettingsListenerCancellation?.Dispose();
        _keyboardRemappingService?.Dispose();
        _mouseGestureService?.Dispose();
        _trayIconService?.Dispose();
        _logger?.Dispose();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }

    private static void RequestSettingsWindow()
    {
        try
        {
            using var showSettingsEvent = EventWaitHandle.OpenExisting(ShowSettingsEventName);
            showSettingsEvent.Set();
        }
        catch (WaitHandleCannotBeOpenedException)
        {
            // The first instance is still starting or is shutting down.
        }
    }

    private void ListenForSettingsRequests(EventWaitHandle showSettingsEvent, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (showSettingsEvent.WaitOne(millisecondsTimeout: 100))
                {
                    _ = Dispatcher.BeginInvoke(ShowSettingsWindow);
                }
            }
        }
        catch (ObjectDisposedException)
        {
            // Application shutdown disposes the event while the listener is waiting.
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _logger?.Error("Unhandled dispatcher exception.", e.Exception);
        e.Handled = true;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            _logger?.Error("Unhandled application exception.", exception);
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _logger?.Error("Unobserved task exception.", e.Exception);
        e.SetObserved();
    }

    private void ShowSettingsWindow()
    {
        if (_configurationStore is null)
        {
            return;
        }

        if (_settingsWindow is null)
        {
            _settingsWindow = new SettingsWindow(_configurationStore, ApplyConfiguration);
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        }

        _settingsWindow.Show();
        _settingsWindow.Activate();
    }

    private void ExitApplication()
    {
        Shutdown();
    }

    private void ApplyConfiguration(AppConfiguration configuration)
    {
        _keyboardRemappingService?.ApplyConfiguration(configuration);
        _mouseGestureService?.ApplyConfiguration(configuration);
        _windowsStartupService?.Apply(configuration.Startup.StartWithWindows);
    }
}
