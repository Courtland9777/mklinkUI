using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MklinkUi.Core;
using MklinkUi.WebUI;
using Xunit;

namespace MklinkUi.Tests;

public class ServiceRegistrationTests
{
    [Fact]
    public void AddPlatformServices_ShouldUseDefaultService_WhenAssemblyMissing()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.Configure<SymlinkOptions>(_ => { });

        var originalBase = AppContext.BaseDirectory;
        var tempDir = Directory.CreateTempSubdirectory();
        AppDomain.CurrentDomain.SetData("APP_CONTEXT_BASE_DIRECTORY", tempDir.FullName);

        try
        {
            services.AddPlatformServices();
            using var provider = services.BuildServiceProvider();
            var sym = provider.GetRequiredService<ISymlinkService>();
            sym.GetType().Name.Should().Be("DefaultSymlinkService");
        }
        finally
        {
            AppDomain.CurrentDomain.SetData("APP_CONTEXT_BASE_DIRECTORY", originalBase);
            tempDir.Delete();
        }
    }

    [Fact]
    public void AddPlatformServices_ShouldLoadFakeService_OnNonWindows()
    {
        if (OperatingSystem.IsWindows())
            return;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.Configure<SymlinkOptions>(_ => { });

        services.AddPlatformServices();
        using var provider = services.BuildServiceProvider();
        var sym = provider.GetRequiredService<ISymlinkService>();
        sym.GetType().Name.Should().Be("FakeSymlinkService");
    }
}
