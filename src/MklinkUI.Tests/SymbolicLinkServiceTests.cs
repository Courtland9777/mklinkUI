using System.ComponentModel;
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

        var result = service.CreateSymbolicLink("source", "dest", false);

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

        var result = service.CreateSymbolicLink("source", "dest", false);

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
}

