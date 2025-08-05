#if WINDOWS
using System.IO;
using FluentAssertions;
using Moq;
using MklinkUI.App;
using MklinkUI.Core.Services;

namespace MklinkUI.Tests;

public class MainViewModelTests
{
    [Fact]
    public void CreateLinkCommand_Disabled_WhenPathsInvalid()
    {
        var dev = new Mock<IDeveloperModeService>();
        var link = new Mock<ISymbolicLinkService>();
        var vm = new MainViewModel(dev.Object, link.Object);

        vm.SourcePath = "relative";
        vm.DestinationPath = "relative";

        vm.CreateLinkCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void StatusMessage_ShowsError_WhenCreationFails()
    {
        var dev = new Mock<IDeveloperModeService>();
        var link = new Mock<ISymbolicLinkService>();
        link.Setup(s => s.CreateSymbolicLink(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(new SymbolicLinkResult(false, "error", 0));
        var vm = new MainViewModel(dev.Object, link.Object);

        vm.SourcePath = Path.GetFullPath("/tmp/a.txt");
        File.WriteAllText(vm.SourcePath, "a");
        vm.DestinationPath = Path.GetFullPath("/tmp/b.txt");

        vm.CreateLinkCommand.Execute(null);

        vm.StatusMessage.Should().Be("error");
    }
}
#endif
