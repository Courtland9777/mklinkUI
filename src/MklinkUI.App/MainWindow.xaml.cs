using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Forms = System.Windows.Forms;
using MklinkUI.Core.Services;
using MklinkUI.Core.Settings;
using Application = System.Windows.Application;

namespace MklinkUI.App;

public partial class MainWindow : Window
{
    private readonly Forms.NotifyIcon _notifyIcon;
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        var registry = new WindowsRegistry();
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var settingsPath = Path.Combine(appData, "MklinkUI", "settings.json");
        var settingsService = new SettingsService(settingsPath);
        var themeService = new ThemeService(registry);
        var themeManager = new ThemeManager();

        var developerModeService = new DeveloperModeService(registry);
        var native = new WindowsSymbolicLink();
        var linkService = new SymbolicLinkService(native);
        _viewModel = new MainViewModel(developerModeService, linkService, settingsService, themeService, themeManager);
        DataContext = _viewModel;

        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Visible = false,
            Text = "MklinkUI"
        };
        _notifyIcon.DoubleClick += (_, _) => ShowMainWindow();
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Open", null, (_, _) => ShowMainWindow());
        menu.Items.Add("Exit", null, (_, _) => { _notifyIcon.Visible = false; Application.Current.Shutdown(); });
        _notifyIcon.ContextMenuStrip = menu;

        StateChanged += MainWindow_StateChanged;
        Closing += MainWindow_Closing;
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel.StartMinimizedToTray)
            HideWindowToTray();
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
            HideWindowToTray();
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        HideWindowToTray();
    }

    private void HideWindowToTray()
    {
        Hide();
        _notifyIcon.Visible = true;
    }

    private void ShowMainWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        _notifyIcon.Visible = false;
    }

    private void PathBox_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
    {
        e.Handled = true;
        e.Effects = e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop)
            ? System.Windows.DragDropEffects.Copy
            : System.Windows.DragDropEffects.None;
    }

    private void SourceBox_Drop(object sender, System.Windows.DragEventArgs e)
        => HandleDrop(e, p => _viewModel.SourcePath = p);

    private void DestinationBox_Drop(object sender, System.Windows.DragEventArgs e)
        => HandleDrop(e, p => _viewModel.DestinationPath = p);

    private void HandleDrop(System.Windows.DragEventArgs e, Action<string> setPath)
    {
        if (e.Data.GetData(System.Windows.DataFormats.FileDrop) is string[] files && files.Length == 1)
            setPath(files[0]);
        else
            _viewModel.StatusMessage = "Invalid drop target.";
    }
}
