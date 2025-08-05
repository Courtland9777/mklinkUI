using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;
using MklinkUI.Core.Services;

namespace MklinkUI.App;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IDeveloperModeService _developerModeService;
    private readonly ISymbolicLinkService _symbolicLinkService;
    private readonly string _logPath;

    public MainViewModel(IDeveloperModeService developerModeService, ISymbolicLinkService symbolicLinkService)
    {
        _developerModeService = developerModeService;
        _symbolicLinkService = symbolicLinkService;
        DeveloperModeStatus = _developerModeService.IsDeveloperModeEnabled()
            ? "Developer Mode is enabled"
            : "Developer Mode is disabled";

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _logPath = Path.Combine(appData, "MklinkUI", "app.log");

        BrowseSourceCommand = new RelayCommand(BrowseSource);
        BrowseDestinationCommand = new RelayCommand(BrowseDestination);
        CreateLinkCommand = new RelayCommand(CreateLink, CanCreateLink);
        UpdateLogContent();
    }

    private string _sourcePath = string.Empty;
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
            }
        }
    }

    private string _destinationPath = string.Empty;
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
            }
        }
    }

    private bool _isDirectory;
    public bool IsDirectory
    {
        get => _isDirectory;
        set
        {
            if (_isDirectory != value)
            {
                _isDirectory = value;
                OnPropertyChanged();
            }
        }
    }

    public string DeveloperModeStatus { get; }

    private string _logContent = string.Empty;
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

    private void BrowseSource()
    {
        if (IsDirectory)
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                SourcePath = dialog.SelectedPath;
        }
        else
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            if (dialog.ShowDialog() == true)
                SourcePath = dialog.FileName;
        }
    }

    private void BrowseDestination()
    {
        if (IsDirectory)
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                DestinationPath = dialog.SelectedPath;
        }
        else
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            if (dialog.ShowDialog() == true)
                DestinationPath = dialog.FileName;
        }
    }

    private void CreateLink()
    {
        _symbolicLinkService.CreateSymbolicLink(SourcePath, DestinationPath, IsDirectory);
        UpdateLogContent();
    }

    private bool CanCreateLink() => !string.IsNullOrWhiteSpace(SourcePath) && !string.IsNullOrWhiteSpace(DestinationPath);

    private void UpdateLogContent()
    {
        if (File.Exists(_logPath))
        {
            var lines = File.ReadLines(_logPath).TakeLast(20);
            LogContent = string.Join(Environment.NewLine, lines);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void RaiseCanExecuteChanged()
    {
        if (CreateLinkCommand is RelayCommand rc)
            rc.RaiseCanExecuteChanged();
    }
}

