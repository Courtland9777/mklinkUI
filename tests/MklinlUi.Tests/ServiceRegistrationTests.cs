using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MklinlUi.Core;
using MklinlUi.WebUI;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace MklinlUi.Tests;

public class ServiceRegistrationTests
{
    [Fact]
    public void AddPlatformServices_RegistersDefaults_WhenAssemblyMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        var logger = NullLogger.Instance;

        var originalBase = AppContext.BaseDirectory;
        var tempDir = Directory.CreateTempSubdirectory();
        AppDomain.CurrentDomain.SetData("APP_CONTEXT_BASE_DIRECTORY", tempDir.FullName);

        try
        {
            // Act
            services.AddPlatformServices(logger);
            using var provider = services.BuildServiceProvider();
            var dev = provider.GetRequiredService<IDeveloperModeService>();
            var sym = provider.GetRequiredService<ISymlinkService>();

            // Assert
            dev.GetType().Name.Should().Be("DefaultDeveloperModeService");
            sym.GetType().Name.Should().Be("DefaultSymlinkService");
        }
        finally
        {
            AppDomain.CurrentDomain.SetData("APP_CONTEXT_BASE_DIRECTORY", originalBase);
            tempDir.Delete();
        }
    }
}
