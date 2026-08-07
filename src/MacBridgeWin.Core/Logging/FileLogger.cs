namespace MacBridgeWin.Core.Logging;

public sealed class FileLogger : IDisposable
{
    private readonly StreamWriter _writer;
    private readonly object _syncRoot = new();

    private FileLogger(string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        _writer = new StreamWriter(new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read))
        {
            AutoFlush = true
        };
    }

    public static FileLogger CreateDefault(string applicationName)
    {
        var logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            applicationName,
            "Logs");
        var logPath = Path.Combine(logDirectory, "macbridgewin.log");
        return new FileLogger(logPath);
    }

    public void Info(string message)
    {
        Write("INFO", message);
    }

    public void Error(string message, Exception exception)
    {
        Write("ERROR", $"{message} {exception}");
    }

    public void Dispose()
    {
        lock (_syncRoot)
        {
            _writer.Dispose();
        }
    }

    private void Write(string level, string message)
    {
        lock (_syncRoot)
        {
            _writer.WriteLine($"{DateTimeOffset.Now:O} [{level}] {message}");
        }
    }
}
