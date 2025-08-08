using FluentAssertions;
using Moq;
using MklinlUi.Core;
using Xunit;

namespace MklinlUi.Tests;

public class SymlinkManagerTests
{
    [Fact]
    public async Task CreateSymlinkAsync_returns_failure_when_developer_mode_disabled()
    {
        var devService = new Mock<IDeveloperModeService>();
        devService.Setup(d => d.IsEnabledAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var symlinkService = new Mock<ISymlinkService>();

        var manager = new SymlinkManager(devService.Object, symlinkService.Object);

        var result = await manager.CreateSymlinkAsync("/link", "/target");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Developer mode not enabled.");
        symlinkService.Verify(s => s.CreateSymlinkAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateSymlinkAsync_invokes_service_when_developer_mode_enabled()
    {
        var devService = new Mock<IDeveloperModeService>();
        devService.Setup(d => d.IsEnabledAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var symlinkService = new Mock<ISymlinkService>();
        symlinkService.Setup(s => s.CreateSymlinkAsync("/link", "/target", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new SymlinkResult(true));

        var manager = new SymlinkManager(devService.Object, symlinkService.Object);
        var result = await manager.CreateSymlinkAsync("/link", "/target");

        result.Success.Should().BeTrue();
        symlinkService.Verify(s => s.CreateSymlinkAsync("/link", "/target", It.IsAny<CancellationToken>()), Times.Once);
    }
}
