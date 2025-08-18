using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MklinkUi.Core;
using MklinkUi.WebUI;
using Xunit;

namespace MklinkUi.Tests;

public class OptionsTests
{
    [Fact]
    public void ServerOptions_Bind_And_Validate()
    {
        var dict = new Dictionary<string, string?>
        {
            ["Server:PreferredPortRange"] = "5000-5010",
            ["Server:DefaultHttpPort"] = "5001",
            ["Server:DefaultHttpsPort"] = "5002"
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
        var services = new ServiceCollection();
        services.AddOptions<ServerOptions>().Bind(config.GetSection("Server")).ValidateDataAnnotations().ValidateOnStart();
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ServerOptions>>().Value;
        options.DefaultHttpPort.Should().Be(5001);
        options.DefaultHttpsPort.Should().Be(5002);
        options.PreferredPortRange.Should().Be("5000-5010");
    }

    [Fact]
    public void ServerOptions_InvalidRange_Throws()
    {
        var dict = new Dictionary<string, string?>
        {
            ["Server:PreferredPortRange"] = "notarange",
            ["Server:DefaultHttpPort"] = "80",
            ["Server:DefaultHttpsPort"] = "81"
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
        var services = new ServiceCollection();
        services.AddOptions<ServerOptions>().Bind(config.GetSection("Server")).ValidateDataAnnotations().ValidateOnStart();
        using var provider = services.BuildServiceProvider();
        Action act = () => { _ = provider.GetRequiredService<IOptions<ServerOptions>>().Value; };
        act.Should().Throw<OptionsValidationException>();
    }
}
