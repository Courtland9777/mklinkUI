using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MklinlUi.Core;
using MklinlUi.Fakes;
using MklinlUi.WebUI;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace MklinlUi.Tests;

public class ServiceRegistrationTests
{
    [Fact]
    public void AddPlatformServices_loads_fake_implementations()
    {
        var services = new ServiceCollection();
        services.AddPlatformServices(NullLogger.Instance);

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IDeveloperModeService>()
            .Should().BeOfType<FakeDeveloperModeService>();
        provider.GetRequiredService<ISymlinkService>()
            .Should().BeOfType<FakeSymlinkService>();
    }

    [Fact]
    public void AddPlatformServices_uses_defaults_when_assembly_missing()
    {
        var assemblyPath = Path.Combine(AppContext.BaseDirectory, "MklinlUi.Fakes.dll");
        var backupPath = assemblyPath + ".bak";
        File.Move(assemblyPath, backupPath);
        try
        {
            var services = new ServiceCollection();
            services.AddPlatformServices(NullLogger.Instance);

            using var provider = services.BuildServiceProvider();
            provider.GetRequiredService<IDeveloperModeService>().GetType().FullName
                .Should().Be("MklinlUi.WebUI.ServiceRegistration+DefaultDeveloperModeService");
            provider.GetRequiredService<ISymlinkService>().GetType().FullName
                .Should().Be("MklinlUi.WebUI.ServiceRegistration+DefaultSymlinkService");
        }
        finally
        {
            File.Move(backupPath, assemblyPath);
        }
    }

}
