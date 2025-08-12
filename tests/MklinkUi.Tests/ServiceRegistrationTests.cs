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

    [Theory]
    [InlineData(null, true)]
    [InlineData("true", true)]
    [InlineData("1", true)]
    [InlineData("false", false)]
    [InlineData("0", false)]
    public async Task DefaultDeveloperModeService_Reads_EnvironmentVariable(string? value, bool expected)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var originalBase = AppContext.BaseDirectory;
        var tempDir = Directory.CreateTempSubdirectory();
        AppDomain.CurrentDomain.SetData("APP_CONTEXT_BASE_DIRECTORY", tempDir.FullName);

        var originalEnv = Environment.GetEnvironmentVariable("MKLINKUI_DEVELOPER_MODE");
        Environment.SetEnvironmentVariable("MKLINKUI_DEVELOPER_MODE", value);

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
            Environment.SetEnvironmentVariable("MKLINKUI_DEVELOPER_MODE", originalEnv);
        }
    }

    [Fact]
    public async Task DefaultDeveloperModeService_Honors_Cancellation()
    {
        var services = new ServiceCollection();
        services.AddLogging();

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

            Func<Task> batch = () => sym.CreateFileSymlinksAsync(new[] { "a" }, "dest", cts.Token);
            await batch.Should().ThrowAsync<OperationCanceledException>();
        }
        finally
        {
            AppDomain.CurrentDomain.SetData("APP_CONTEXT_BASE_DIRECTORY", originalBase);
            tempDir.Delete();
        }
    }
}
