using MacBridgeWin.Core.Configuration;
using MacBridgeWin.Core.Gestures;
using MacBridgeWin.Core.Logging;
using MacBridgeWin.Core.Profiles;

namespace MacBridgeWin.App.Services.Mouse;

public sealed class MouseGestureService : IDisposable
{
    private readonly BasicDirectionalGestureRecognizer _recognizer = new();
    private readonly SyntheticMouseInputSender _mouseInputSender = new();
    private readonly MouseGestureFeedbackService _feedbackService;
    private readonly FileLogger _logger;
    private readonly ProfileResolver _profileResolver = new();
    private readonly Func<string?> _getActiveProcessName;
    private readonly Func<int, int, string?> _getProcessNameFromPoint;
    private MouseHookService? _hookService;
    private volatile AppConfiguration _appConfiguration = ConfigurationDefaults.Create();
    private MouseGestureConfiguration _configuration = new();
    private bool _enabled;

    public MouseGestureService(
        FileLogger logger,
        Func<string?> getActiveProcessName,
        Func<int, int, string?> getProcessNameFromPoint,
        MouseGestureFeedbackService feedbackService)
    {
        _logger = logger;
        _getActiveProcessName = getActiveProcessName;
        _getProcessNameFromPoint = getProcessNameFromPoint;
        _feedbackService = feedbackService;
    }

    public void ApplyConfiguration(AppConfiguration configuration)
    {
        _appConfiguration = configuration;
        _enabled = configuration.Features.MouseGesturesEnabled;

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
        if (_hookService is not null)
        {
            return;
        }

        _hookService = new MouseHookService(
            Recognize,
            Execute,
            _mouseInputSender.SendRightClick,
            _feedbackService.Begin,
            _feedbackService.AddPoint,
            _feedbackService.End);
        _hookService.Start();
        _logger.Info("Mouse gestures enabled.");
    }

    private void Stop()
    {
        if (_hookService is null)
        {
            return;
        }

        _hookService.Dispose();
        _hookService = null;
        _logger.Info("Mouse gestures disabled.");
    }

    private GestureRecognitionResult Recognize(IReadOnlyList<GesturePoint> points)
    {
        var configuration = _appConfiguration;
        // A movement below every possible profile threshold cannot be a gesture.
        // Reject it before querying another process or merging profile bindings.
        var minimumThreshold = configuration.MouseGestures.MovementThresholdPixels;
        foreach (var profile in configuration.Profiles)
        {
            var threshold = profile.MouseGestures.MovementThresholdPixels;
            if (threshold > 0) minimumThreshold = Math.Min(minimumThreshold, threshold);
        }
        if (minimumThreshold > 0 && !BasicDirectionalGestureRecognizer.HasThresholdMovement(points, minimumThreshold))
        {
            return GestureRecognitionResult.None;
        }
        var startPoint = points.Count > 0 ? points[0] : new GesturePoint(0, 0);
        var processName = configuration.Profiles.Count == 0 ? null
            : _getProcessNameFromPoint(startPoint.X, startPoint.Y) ?? _getActiveProcessName();
        var resolved = _profileResolver.Resolve(configuration, processName);
        _configuration = resolved.MouseGestures;
        var sequence = _recognizer.RecognizeSequence(points, _configuration.MovementThresholdPixels);
        return GestureBindingResolver.Resolve(_configuration, sequence) is null
            ? GestureRecognitionResult.None : GestureRecognitionResult.Recognized(sequence);
    }

    private void Execute(IReadOnlyList<GestureDirection> sequence)
    {
        _ = ExecuteAfterMouseReleaseAsync(sequence.ToArray(), _configuration);
    }

    private async Task ExecuteAfterMouseReleaseAsync(IReadOnlyList<GestureDirection> sequence, MouseGestureConfiguration configuration)
    {
        await Task.Delay(60).ConfigureAwait(false);
        var executor = new GestureActionExecutor(_logger);
        executor.ApplyConfiguration(configuration);
        executor.Execute(sequence);
    }
}
