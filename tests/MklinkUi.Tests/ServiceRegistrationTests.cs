using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

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

    [Theory]
    [InlineData(null, false)]
    [InlineData("true", true)]
    [InlineData("1", true)]
    [InlineData("false", false)]
    [InlineData("0", false)]
    [InlineData("invalid", false)]
    public async Task DefaultDeveloperModeService_Reads_Configuration(string? value, bool expected)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var configBuilder = new ConfigurationBuilder();
        if (value is not null)
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DeveloperMode"] = value
            });
        }
        services.AddSingleton<IConfiguration>(configBuilder.Build());

        var originalBase = AppContext.BaseDirectory;
        var tempDir = Directory.CreateTempSubdirectory();
        AppDomain.CurrentDomain.SetData("APP_CONTEXT_BASE_DIRECTORY", tempDir.FullName);

        try
        {
            services.AddPlatformServices();
            using var provider = services.BuildServiceProvider();
            var dev = provider.GetRequiredService<IDeveloperModeService>();
            var enabled = await dev.IsEnabledAsync();
            enabled.Should().Be(expected);
        }
        finally
        {
            AppDomain.CurrentDomain.SetData("APP_CONTEXT_BASE_DIRECTORY", originalBase);
            tempDir.Delete();
        }
    }

    [Fact]
    public async Task DefaultDeveloperModeService_Honors_Cancellation()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        var originalBase = AppContext.BaseDirectory;
        var tempDir = Directory.CreateTempSubdirectory();
        AppDomain.CurrentDomain.SetData("APP_CONTEXT_BASE_DIRECTORY", tempDir.FullName);

        try
        {
            services.AddPlatformServices();
            using var provider = services.BuildServiceProvider();
            var dev = provider.GetRequiredService<IDeveloperModeService>();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Func<Task> act = () => dev.IsEnabledAsync(cts.Token);
            await act.Should().ThrowAsync<OperationCanceledException>();
        }
        finally
        {
            AppDomain.CurrentDomain.SetData("APP_CONTEXT_BASE_DIRECTORY", originalBase);
            tempDir.Delete();
        }
    }

    [Fact]
    public async Task DefaultSymlinkService_Honors_Cancellation()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        var originalBase = AppContext.BaseDirectory;
        var tempDir = Directory.CreateTempSubdirectory();
        AppDomain.CurrentDomain.SetData("APP_CONTEXT_BASE_DIRECTORY", tempDir.FullName);

        try
        {
            services.AddPlatformServices();
            using var provider = services.BuildServiceProvider();
            var sym = provider.GetRequiredService<ISymlinkService>();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Func<Task> single = () => sym.CreateSymlinkAsync("a", "b", cts.Token);
            await single.Should().ThrowAsync<OperationCanceledException>();

            Func<Task> batch = () => sym.CreateFileSymlinksAsync(["a"], "dest", cts.Token);
            await batch.Should().ThrowAsync<OperationCanceledException>();
        }
        finally
        {
            AppDomain.CurrentDomain.SetData("APP_CONTEXT_BASE_DIRECTORY", originalBase);
            tempDir.Delete();
        }
    }
}