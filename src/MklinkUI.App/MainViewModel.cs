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
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (string.IsNullOrWhiteSpace(environment))
        {
            throw new InvalidOperationException("ASPNETCORE_ENVIRONMENT is not set. Please set the ASPNETCORE_ENVIRONMENT environment variable.");
        }

        var logFile = Path.Combine(appData, "MklinkUI", environment, "app.log");
        LogContent = File.Exists(logFile) ? File.ReadAllText(logFile) : string.Empty;
    }
}
