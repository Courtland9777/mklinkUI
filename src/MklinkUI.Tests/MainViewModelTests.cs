#if WINDOWS
using System.IO;
using FluentAssertions;
using Moq;
using MklinkUI.App;
using MklinkUI.Core.Services;
using MklinkUI.Core.Settings;

namespace MklinkUI.Tests;

public class MainViewModelTests
{
    private static MainViewModel CreateViewModel(Mock<IDeveloperModeService>? dev = null, Mock<ISymbolicLinkService>? link = null)
    {
        dev ??= new Mock<IDeveloperModeService>();
        link ??= new Mock<ISymbolicLinkService>();
        var settings = new Mock<ISettingsService>();
        settings.Setup(s => s.Load()).Returns(new AppSettings());
        var theme = new Mock<IThemeService>();
        theme.Setup(t => t.ResolveTheme(It.IsAny<ThemeOption>())).Returns(ThemeOption.Light);
        return new MainViewModel(dev.Object, link.Object, settings.Object, theme.Object, new ThemeManager());
    }

    [Fact]
    public void CreateLinkCommand_Disabled_WhenPathsInvalid()
    {
        var vm = CreateViewModel();

        vm.SourcePath = "relative";
        vm.DestinationPath = "relative";

        vm.CreateLinkCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void StatusMessage_ShowsError_WhenCreationFails()
    {
        var link = new Mock<ISymbolicLinkService>();
        link.Setup(s => s.CreateSymbolicLink(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(new SymbolicLinkResult(false, "error", 0));
        var vm = CreateViewModel(link: link);

        vm.SourcePath = Path.GetFullPath("/tmp/a.txt");
        File.WriteAllText(vm.SourcePath, "a");
        vm.DestinationPath = Path.GetFullPath("/tmp/b.txt");

        vm.CreateLinkCommand.Execute(null);

        vm.StatusMessage.Should().Be("error");
    }
}
#endif
