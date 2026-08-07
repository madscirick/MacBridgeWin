using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace MacBridgeWin.App;

public partial class AppSelectionDialog : Window, INotifyPropertyChanged
{
    private RunningApplicationOption? _selectedApplication;

    public AppSelectionDialog()
    {
        InitializeComponent();
        Applications = new ObservableCollection<RunningApplicationOption>(LoadApplications());
        SelectedApplication = Applications.FirstOrDefault();
        DataContext = this;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<RunningApplicationOption> Applications { get; }

    public RunningApplicationOption? SelectedApplication
    {
        get => _selectedApplication;
        set
        {
            if (_selectedApplication == value)
            {
                return;
            }

            _selectedApplication = value;
            OnPropertyChanged();
        }
    }

    private void SelectButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedApplication is null)
        {
            return;
        }

        DialogResult = true;
    }

    private void ApplicationsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (SelectedApplication is not null)
        {
            DialogResult = true;
        }
    }

    private static IReadOnlyList<RunningApplicationOption> LoadApplications()
    {
        var currentProcessId = Environment.ProcessId;
        return Process.GetProcesses()
            .Where(process => process.Id != currentProcessId)
            .Select(TryCreateOption)
            .Where(option => option is not null)
            .Select(option => option!)
            .GroupBy(option => option.ProcessName, StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderByDescending(option => option.Title.Length)
                .ThenBy(option => option.DisplayName, StringComparer.CurrentCultureIgnoreCase)
                .First())
            .OrderBy(option => option.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private static RunningApplicationOption? TryCreateOption(Process process)
    {
        try
        {
            if (process.MainWindowHandle == IntPtr.Zero)
            {
                return null;
            }

            var title = process.MainWindowTitle.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            var processName = process.ProcessName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                ? process.ProcessName
                : $"{process.ProcessName}.exe";
            return new RunningApplicationOption(
                DisplayName: GuessDisplayName(process.ProcessName, title),
                ProcessName: processName,
                Title: title);
        }
        catch
        {
            return null;
        }
        finally
        {
            process.Dispose();
        }
    }

    private static string GuessDisplayName(string processName, string title)
    {
        if (processName.Equals("chrome", StringComparison.OrdinalIgnoreCase))
        {
            return "Google Chrome";
        }

        if (processName.Equals("Acrobat", StringComparison.OrdinalIgnoreCase))
        {
            return "Adobe Acrobat";
        }

        if (processName.Equals("AcroRd32", StringComparison.OrdinalIgnoreCase))
        {
            return "Adobe Acrobat Reader";
        }

        return string.IsNullOrWhiteSpace(title) ? processName : title;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed record RunningApplicationOption(string DisplayName, string ProcessName, string Title)
{
    public string Detail => $"{ProcessName} - {Title}";
}
