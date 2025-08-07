using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using MklinkUI.Core.Services;
using MklinkUI.Core.Settings;
using Forms = System.Windows.Forms;

namespace MklinkUI.App;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IDeveloperModeService _developerModeService;
    private readonly bool _isElevated;
    private readonly string _logPath;
    private readonly ISettingsService _settingsService;
    private readonly ISymbolicLinkService _symbolicLinkService;
    private readonly ThemeManager _themeManager;
    private readonly IThemeService _themeService;

    private string _destinationPath = string.Empty;

    private string _developerModeStatus = string.Empty;

    private bool _isDirectory;

    private string _logContent = string.Empty;

    private ThemeOption _selectedTheme;

    private string _sourcePath = string.Empty;

    private bool _startMinimizedToTray;

    private string _statusMessage = string.Empty;

    public MainViewModel(IDeveloperModeService developerModeService,
        ISymbolicLinkService symbolicLinkService,
        ISettingsService settingsService,
        IThemeService themeService,
        ThemeManager themeManager)
    {
        _developerModeService = developerModeService;
        _symbolicLinkService = symbolicLinkService;
        _settingsService = settingsService;
        _themeService = themeService;
        _themeManager = themeManager;

        DeveloperModeStatus = _developerModeService.IsDeveloperModeEnabled()
            ? "Developer Mode is enabled"
            : "Developer Mode is disabled (enables symbolic link creation without administrator rights)";

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _logPath = Path.Combine(appData, "MklinkUI", "app.log");

        _isElevated = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        ElevationStatus = _isElevated
            ? "Running as Administrator"
            : "Not running as Administrator (required unless Developer Mode is enabled)";

        BrowseSourceCommand = new RelayCommand(BrowseSource);
        BrowseDestinationCommand = new RelayCommand(BrowseDestination);
        CreateLinkCommand = new RelayCommand(CreateLink, CanCreateLink);
        OpenLogCommand = new RelayCommand(OpenLogFolder);
        RelaunchElevatedCommand = new RelayCommand(RelaunchElevated, () => !_isElevated);
        OpenDeveloperModeSettingsCommand = new RelayCommand(OpenDeveloperModeSettings);
        RefreshDeveloperModeCommand = new RelayCommand(RefreshDeveloperMode);

        Themes = Enum.GetValues<ThemeOption>();
        var settings = _settingsService.Load();
        _selectedTheme = settings.Theme;
        _startMinimizedToTray = settings.StartMinimizedToTray;
        _themeManager.Apply(_themeService.ResolveTheme(_selectedTheme));

        UpdateLogContent();
    }

    public ThemeOption[] Themes { get; }

    public ThemeOption SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (_selectedTheme != value)
            {
                _selectedTheme = value;
                OnPropertyChanged();
                ApplyAndSaveTheme();
            }
        }
    }

    public bool StartMinimizedToTray
    {
        get => _startMinimizedToTray;
        set
        {
            if (_startMinimizedToTray != value)
            {
                _startMinimizedToTray = value;
                OnPropertyChanged();
                SaveSettings();
            }
        }
    }

    public string SourcePath
    {
        get => _sourcePath;
        set
        {
            if (_sourcePath != value)
            {
                _sourcePath = value;
                OnPropertyChanged();
                RaiseCanExecuteChanged();
                StatusMessage = ValidateInput().Message ?? string.Empty;
            }
        }
    }

    public string DestinationPath
    {
        get => _destinationPath;
        set
        {
            if (_destinationPath != value)
            {
                _destinationPath = value;
                OnPropertyChanged();
                RaiseCanExecuteChanged();
                StatusMessage = ValidateInput().Message ?? string.Empty;
            }
        }
    }

    public bool IsDirectory
    {
        get => _isDirectory;
        set
        {
            if (_isDirectory != value)
            {
                _isDirectory = value;
                OnPropertyChanged();
                RaiseCanExecuteChanged();
            }
        }
    }

    public string DeveloperModeStatus
    {
        get => _developerModeStatus;
        private set
        {
            _developerModeStatus = value;
            OnPropertyChanged();
        }
    }

    public string ElevationStatus { get; }

    public string StatusMessage
    {
        get => _statusMessage;
        internal set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public string LogContent
    {
        get => _logContent;
        private set
        {
            _logContent = value;
            OnPropertyChanged();
        }
    }

    public ICommand BrowseSourceCommand { get; }
    public ICommand BrowseDestinationCommand { get; }
    public ICommand CreateLinkCommand { get; }
    public ICommand OpenLogCommand { get; }
    public ICommand RelaunchElevatedCommand { get; }
    public ICommand OpenDeveloperModeSettingsCommand { get; }
    public ICommand RefreshDeveloperModeCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void ApplyAndSaveTheme()
    {
        var actual = _themeService.ResolveTheme(SelectedTheme);
        _themeManager.Apply(actual);
        SaveSettings();
    }

    private void SaveSettings()
    {
        _settingsService.Save(new AppSettings
        {
            Theme = SelectedTheme,
            StartMinimizedToTray = StartMinimizedToTray
        });
    }

    private void BrowseSource()
    {
        if (IsDirectory)
        {
            using var dialog = new Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == Forms.DialogResult.OK)
                SourcePath = dialog.SelectedPath;
        }
        else
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
                SourcePath = dialog.FileName;
        }
    }

    private void BrowseDestination()
    {
        if (IsDirectory)
        {
            using var dialog = new Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == Forms.DialogResult.OK)
                DestinationPath = dialog.SelectedPath;
        }
        else
        {
            var dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == true)
                DestinationPath = dialog.FileName;
        }
    }

    private void CreateLink()
    {
        var (isValid, message) = ValidateInput();
        if (!isValid)
        {
            StatusMessage = message ?? string.Empty;
            return;
        }

        var result = _symbolicLinkService.CreateSymbolicLink(SourcePath, DestinationPath, IsDirectory);
        if (result.Success)
        {
            StatusMessage = "Symbolic link created successfully.";
            UpdateLogContent();
            return;
        }

        if (result.ErrorCode == 1314)
            HandlePrivilegeError();
        else
            StatusMessage = result.ErrorMessage ?? "Failed to create symbolic link.";

        UpdateLogContent();
    }

    private void HandlePrivilegeError()
    {
        var devModeEnabled = _developerModeService.IsDeveloperModeEnabled();
        StatusMessage = devModeEnabled
            ? "Administrator privileges are required to create this symbolic link."
            : "Developer Mode is disabled and administrator privileges are required to create this symbolic link.";

        if (_isElevated)
            return;

        if (!devModeEnabled)
        {
            var openSettings = MessageBox.Show(
                "Developer Mode is disabled. Enable it in Windows Settings to create symbolic links without administrator privileges. Open Developer Mode settings now?",
                "Developer Mode disabled", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (openSettings == MessageBoxResult.Yes)
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "ms-settings:developers",
                        UseShellExecute = true
                    });
                }
                catch
                {
                    // ignored
                }
        }

        var relaunch = MessageBox.Show(
            "Would you like to relaunch the application with administrative rights?",
            "Elevation required", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (relaunch == MessageBoxResult.Yes && RelaunchElevatedCommand.CanExecute(null))
            RelaunchElevatedCommand.Execute(null);
    }

    private bool CanCreateLink()
    {
        return ValidateInput().IsValid;
    }

    private (bool IsValid, string? Message) ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(SourcePath) || string.IsNullOrWhiteSpace(DestinationPath))
            return (false, "Source and destination paths are required.");

        if (!Path.IsPathFullyQualified(SourcePath) || !Path.IsPathFullyQualified(DestinationPath))
            return (false, "Paths must be absolute.");

        if (IsDirectory)
        {
            if (!Directory.Exists(SourcePath))
                return (false, $"Source directory '{SourcePath}' does not exist.");
            if (Directory.Exists(DestinationPath) || File.Exists(DestinationPath))
                return (false, $"Destination '{DestinationPath}' already exists.");
        }
        else
        {
            if (!File.Exists(SourcePath))
                return (false, $"Source file '{SourcePath}' does not exist.");
            if (File.Exists(DestinationPath) || Directory.Exists(DestinationPath))
                return (false, $"Destination '{DestinationPath}' already exists.");
        }

        return (true, null);
    }

    private void UpdateLogContent()
    {
        if (!File.Exists(_logPath)) return;
        try
        {
            using var stream = new FileStream(_logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            var lines = reader
                .ReadToEnd()
                .Split(Environment.NewLine)
                .TakeLast(20);
            LogContent = string.Join(Environment.NewLine, lines);
        }
        catch (IOException ex)
        {
            // Log or handle the exception as needed
            LogContent = "Unable to read log content: " + ex.Message;
        }
    }

    private void OpenLogFolder()
    {
        var dir = Path.GetDirectoryName(_logPath);
        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            Process.Start(new ProcessStartInfo
            {
                FileName = dir,
                UseShellExecute = true
            });
    }

    private void RelaunchElevated()
    {
        var exe = Environment.ProcessPath ?? string.Empty;
        if (string.IsNullOrEmpty(exe))
            return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                Verb = "runas",
                UseShellExecute = true
            });
            Application.Current.Shutdown();
        }
        catch
        {
            StatusMessage = "Elevation cancelled.";
        }
    }

    private void OpenDeveloperModeSettings()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ms-settings:developers",
                UseShellExecute = true
            });
        }
        catch
        {
            StatusMessage = "Failed to open settings.";
        }
    }

    private void RefreshDeveloperMode()
    {
        _developerModeService.RefreshState();
        DeveloperModeStatus = _developerModeService.IsDeveloperModeEnabled()
            ? "Developer Mode is enabled"
            : "Developer Mode is disabled";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void RaiseCanExecuteChanged()
    {
        if (CreateLinkCommand is RelayCommand rc)
            rc.RaiseCanExecuteChanged();
    }
}