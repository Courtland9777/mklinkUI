using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using Serilog;

namespace MklinkUI.App;

public partial class App : Application
{
    public App()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (string.IsNullOrWhiteSpace(environment))
        {
            var appSettings = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (File.Exists(appSettings))
            {
                using var stream = File.OpenRead(appSettings);
                using var doc = JsonDocument.Parse(stream);
                if (doc.RootElement.TryGetProperty("ASPNETCORE_ENVIRONMENT", out var envProp))
                {
                    environment = envProp.GetString();
                    if (!string.IsNullOrWhiteSpace(environment))
                    {
                        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
                    }
                }
            }
        }

        if (string.IsNullOrWhiteSpace(environment))
        {
            throw new InvalidOperationException("ASPNETCORE_ENVIRONMENT is not set. Please set the ASPNETCORE_ENVIRONMENT environment variable.");
        }

        var logDir = Path.Combine(appData, "MklinkUI", environment);
        Directory.CreateDirectory(logDir);
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(logDir, "app.log"))
            .CreateLogger();

        Log.Information("Application starting in {Environment} environment", environment);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("Application exiting");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
