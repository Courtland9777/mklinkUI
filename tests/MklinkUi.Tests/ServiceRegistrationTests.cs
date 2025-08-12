using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MklinkUi.Core;
using MklinkUi.WebUI;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace MklinkUi.Tests;

public class ServiceRegistrationTests
{
    [Fact]
    public void AddPlatformServices_RegistersDefaults_WhenAssemblyMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var originalBase = AppContext.BaseDirectory;
        var tempDir = Directory.CreateTempSubdirectory();
        AppDomain.CurrentDomain.SetData("APP_CONTEXT_BASE_DIRECTORY", tempDir.FullName);

        try
        {
                // Act
                services.AddPlatformServices();
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
