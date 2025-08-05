using System.Windows;
using MklinkUI.Core.Services;

namespace MklinkUI.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var registry = new WindowsRegistry();
        var developerModeService = new DeveloperModeService(registry);
        var native = new WindowsSymbolicLink();
        var linkService = new SymbolicLinkService(native);
        DataContext = new MainViewModel(developerModeService, linkService);
    }
}

