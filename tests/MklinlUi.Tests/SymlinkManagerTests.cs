using FluentAssertions;
using Microsoft.Extensions.Logging;
using MklinlUi.Core;
using Moq;
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
        var logger = new Mock<ILogger<SymlinkManager>>();

        var manager = new SymlinkManager(devService.Object, symlinkService.Object, logger.Object);

        var result = await manager.CreateSymlinkAsync("/link", "/target");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Developer mode not enabled.");
        symlinkService.Verify(
            s => s.CreateSymlinkAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Developer mode not enabled")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateSymlinkAsync_invokes_service_when_developer_mode_enabled()
    {
        var devService = new Mock<IDeveloperModeService>();
        devService.Setup(d => d.IsEnabledAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var symlinkService = new Mock<ISymlinkService>();
        symlinkService.Setup(s => s.CreateSymlinkAsync("/link", "/target", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SymlinkResult(true));
        var logger = new Mock<ILogger<SymlinkManager>>();

        var manager = new SymlinkManager(devService.Object, symlinkService.Object, logger.Object);
        var result = await manager.CreateSymlinkAsync("/link", "/target");

        result.Success.Should().BeTrue();
        symlinkService.Verify(s => s.CreateSymlinkAsync("/link", "/target", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSymlinkAsync_propagates_exception_when_developer_mode_service_errors()
    {
        var devService = new Mock<IDeveloperModeService>();
        devService.Setup(d => d.IsEnabledAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("registry error"));

        var symlinkService = new Mock<ISymlinkService>();
        var logger = new Mock<ILogger<SymlinkManager>>();

        var manager = new SymlinkManager(devService.Object, symlinkService.Object, logger.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => manager.CreateSymlinkAsync("/link", "/target"));
        symlinkService.Verify(
            s => s.CreateSymlinkAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateFileSymlinksAsync_returns_failure_when_developer_mode_disabled()
    {
        var devService = new Mock<IDeveloperModeService>();
        devService.Setup(d => d.IsEnabledAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var symlinkService = new Mock<ISymlinkService>();
        var logger = new Mock<ILogger<SymlinkManager>>();

        var manager = new SymlinkManager(devService.Object, symlinkService.Object, logger.Object);

        var results = await manager.CreateFileSymlinksAsync(["/src.txt"], "/dest");

        results.Should().HaveCount(1);
        results[0].Success.Should().BeFalse();
        results[0].ErrorMessage.Should().Be("Developer mode not enabled.");
        symlinkService.Verify(
            s => s.CreateFileSymlinksAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Developer mode not enabled")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}