using System;
using System.IO;
using System.Windows;
using Serilog;

namespace MklinkUI.App;

public partial class App : Application
{
    public App()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var logDir = Path.Combine(appData, "MklinkUI");
        Directory.CreateDirectory(logDir);
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(logDir, "app.log"))
            .CreateLogger();

        Log.Information("Application starting");

        InitializeComponent();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("Application exiting");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
