using FluentAssertions;
using Moq;
using MklinkUI.Core.Settings;
using MklinkUI.Core.Services;
using Xunit;

namespace MklinkUI.Tests;

public class ThemeServiceTests
{
    [Theory]
    [InlineData(1, ThemeOption.Light)]
    [InlineData(0, ThemeOption.Dark)]
    public void GetSystemTheme_UsesRegistryValue(int registryValue, ThemeOption expected)
    {
        var registry = new Mock<IRegistry>();
        registry.Setup(r => r.GetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>()))
            .Returns(registryValue);
        var service = new ThemeService(registry.Object);

        service.GetSystemTheme().Should().Be(expected);
    }

    [Fact]
    public void ResolveTheme_ReturnsPreference_WhenNotSystem()
    {
        var registry = new Mock<IRegistry>();
        var service = new ThemeService(registry.Object);
        service.ResolveTheme(ThemeOption.Dark).Should().Be(ThemeOption.Dark);
    }
}
