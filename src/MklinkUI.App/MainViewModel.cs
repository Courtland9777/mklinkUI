using System;
using System.IO;

namespace MklinkUI.App;

public class MainViewModel
{
    public string LogContent { get; }

    public MainViewModel()
    {
        // Load the log file content if it exists; otherwise empty string.
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var logFile = Path.Combine(appData, "MklinkUI", "app.log");
        LogContent = File.Exists(logFile) ? File.ReadAllText(logFile) : string.Empty;
    }
}
