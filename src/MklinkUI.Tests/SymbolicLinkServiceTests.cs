using System.ComponentModel;
using System.IO;
using FluentAssertions;
using Moq;
using MklinkUI.Core.Services;

namespace MklinkUI.Tests;

public class SymbolicLinkServiceTests
{
    [Fact]
    public void CreateSymbolicLink_ReturnsSuccess_WhenNativeSucceeds()
    {
        var native = new Mock<ISymbolicLink>();
        native.Setup(n => n.CreateSymbolicLink(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns((true, 0));
        var service = new SymbolicLinkService(native.Object);
        var src = Path.GetFullPath("/tmp/source.txt");
        var dest = Path.GetFullPath("/tmp/dest.txt");
        File.WriteAllText(src, "test");

        var result = service.CreateSymbolicLink(src, dest, false);

        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void CreateSymbolicLink_ReturnsError_WhenNativeFails()
    {
        var native = new Mock<ISymbolicLink>();
        native.Setup(n => n.CreateSymbolicLink(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns((false, 5));
        var service = new SymbolicLinkService(native.Object);
        var src = Path.GetFullPath("/tmp/source.txt");
        var dest = Path.GetFullPath("/tmp/dest.txt");
        File.WriteAllText(src, "test");

        var result = service.CreateSymbolicLink(src, dest, false);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be(new Win32Exception(5).Message);
    }

    [Fact]
    public void CreateSymbolicLink_ReturnsError_WhenPathsMissing()
    {
        var native = new Mock<ISymbolicLink>();
        var service = new SymbolicLinkService(native.Object);

        var result = service.CreateSymbolicLink("", "", false);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Source and destination paths must be provided.");
    }

    [Fact]
    public void CreateSymbolicLink_ReturnsError_WhenPathsNotAbsolute()
    {
        var native = new Mock<ISymbolicLink>();
        var service = new SymbolicLinkService(native.Object);

        var result = service.CreateSymbolicLink("relative-src", "relative-dest", false);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Paths must be absolute.");
    }

    [Fact]
    public void CreateSymbolicLink_ReturnsError_WhenSourceMissing()
    {
        var native = new Mock<ISymbolicLink>();
        var service = new SymbolicLinkService(native.Object);
        var src = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var dest = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        var result = service.CreateSymbolicLink(src, dest, false);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("does not exist");
    }

    [Fact]
    public void CreateSymbolicLink_ReturnsError_WhenDestinationExists()
    {
        var native = new Mock<ISymbolicLink>();
        var service = new SymbolicLinkService(native.Object);
        var src = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
        File.WriteAllText(src, "test");
        var dest = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
        File.WriteAllText(dest, "existing");

        var result = service.CreateSymbolicLink(src, dest, false);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already exists");
    }
}

