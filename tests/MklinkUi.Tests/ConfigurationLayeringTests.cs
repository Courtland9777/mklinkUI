using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace MklinkUi.Tests;

public class ConfigurationLayeringTests
{
    [Fact]
    public void Configuration_ShouldUsePrefixedEnvironmentVariables_WhenPresent()
    {
        var baseJson = "{\"Server\":{\"DefaultHttpPort\":\"100\"}}";
        var envJson = "{\"Server\":{\"DefaultHttpPort\":\"200\"}}";
        using var baseStream = new MemoryStream(Encoding.UTF8.GetBytes(baseJson));
        using var envStream = new MemoryStream(Encoding.UTF8.GetBytes(envJson));

        Environment.SetEnvironmentVariable("Server__DefaultHttpPort", "300");
        Environment.SetEnvironmentVariable("MKLINKUI__Server__DefaultHttpPort", "400");

        var config = new ConfigurationBuilder()
            .AddJsonStream(baseStream)
            .AddJsonStream(envStream)
            .AddEnvironmentVariables()
            .AddEnvironmentVariables("MKLINKUI__")
            .Build();

        var port = config.GetValue<int>("Server:DefaultHttpPort");
        port.Should().Be(400);

        Environment.SetEnvironmentVariable("Server__DefaultHttpPort", null);
        Environment.SetEnvironmentVariable("MKLINKUI__Server__DefaultHttpPort", null);
    }

    [Theory]
    [InlineData("DOTNET_ENVIRONMENT")]
    [InlineData("ASPNETCORE_ENVIRONMENT")]
    public void EnvironmentVariable_ShouldDriveIsDevelopment_WhenSet(string variable)
    {
        Environment.SetEnvironmentVariable(variable, "Development");

        var envName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
                      Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                      Environments.Production;

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = envName });
        builder.Environment.IsDevelopment().Should().BeTrue();

        Environment.SetEnvironmentVariable(variable, null);
    }
}
