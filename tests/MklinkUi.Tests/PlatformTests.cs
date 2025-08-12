using System.Reflection;
using FluentAssertions;
using Xunit;

namespace MklinkUi.Tests;

public class PlatformTests
{
    [Fact]
    public void NonWindows_production_startup_throws()
    {
        if (OperatingSystem.IsWindows()) return; // Test only relevant on non-Windows systems

        var original = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Production");

        try
        {
            var entry = typeof(global::Program).Assembly.EntryPoint!;
            Action act = () => entry.Invoke(null, new object[] { Array.Empty<string>() });

            act.Should().Throw<TargetInvocationException>()
                .WithInnerException<PlatformNotSupportedException>();
        }
        finally
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", original);
        }
    }
}