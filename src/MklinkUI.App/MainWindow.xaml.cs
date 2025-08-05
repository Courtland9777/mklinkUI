using System.Windows;
using MklinkUI.Core.Services;

namespace MklinkUI.App;

public partial class MainWindow : Window
{
    private readonly IDeveloperModeService _developerModeService;

    public MainWindow()
    {
        InitializeComponent();
        var registry = new WindowsRegistry();
        _developerModeService = new DeveloperModeService(registry);
        DeveloperModeStatus.Text = _developerModeService.IsDeveloperModeEnabled()
            ? "Developer Mode is enabled"
            : "Developer Mode is disabled";
    }
}
