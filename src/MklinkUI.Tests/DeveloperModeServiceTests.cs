using FluentAssertions;
using Moq;
using MklinkUI.Core.Services;
using Xunit;

namespace MklinkUI.Tests;

public class DeveloperModeServiceTests
{
    [Fact]
    public void IsDeveloperModeEnabled_ReturnsTrue_WhenRegistryValueIsOne()
    {
        // Arrange
        var registry = new Mock<IRegistry>();
        registry.Setup(r => r.GetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>()))
            .Returns(1);
        var service = new DeveloperModeService(registry.Object);

        // Act
        var result = service.IsDeveloperModeEnabled();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RefreshState_RechecksRegistry()
    {
        var registry = new Mock<IRegistry>();
        registry.SetupSequence(r => r.GetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>()))
            .Returns(0)
            .Returns(1);
        var service = new DeveloperModeService(registry.Object);

        service.IsDeveloperModeEnabled().Should().BeFalse();

        service.RefreshState();

        service.IsDeveloperModeEnabled().Should().BeTrue();
    }
}
