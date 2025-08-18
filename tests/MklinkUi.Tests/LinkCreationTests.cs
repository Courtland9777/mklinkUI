using FluentAssertions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MklinkUi.Core;
using MklinkUi.Fakes;
using System.IO;
using Xunit;

namespace MklinkUi.Tests;

public class LinkCreationTests
{
    [Fact]
    public async Task CreateFileLinkAsync_creates_link()
    {
        var service = new FakeSymlinkService();
        var env = new FakeHostEnvironment();
        var manager = new SymlinkManager(env, service, Options.Create(new SymlinkOptions()), NullLogger<SymlinkManager>.Instance);

        var result = await manager.CreateFileLinkAsync("/src/a.txt", "/dest");

        result.Success.Should().BeTrue();
        service.Created.Should().Contain((Path.Combine("/dest", "a.txt"), "/src/a.txt"));
    }

    [Fact]
    public async Task CreateFileLinkAsync_returns_error_on_collision()
    {
        var service = new FakeSymlinkService();
        var env = new FakeHostEnvironment();
        var manager = new SymlinkManager(env, service, Options.Create(new SymlinkOptions()), NullLogger<SymlinkManager>.Instance);

        await manager.CreateFileLinkAsync("/src/a.txt", "/dest");
        var result = await manager.CreateFileLinkAsync("/src/a.txt", "/dest");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Link already exists.");
    }

    [Fact]
    public async Task CreateFileLinkAsync_returns_error_for_relative_paths()
    {
        var service = new FakeSymlinkService();
        var env = new FakeHostEnvironment();
        var manager = new SymlinkManager(env, service, Options.Create(new SymlinkOptions()), NullLogger<SymlinkManager>.Instance);

        var result = await manager.CreateFileLinkAsync("file.txt", "/dest");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Paths must be absolute.");
    }

    [Fact]
    public async Task CreateDirectoryLinksAsync_creates_links_for_each_folder()
    {
        var service = new FakeSymlinkService();
        var env = new FakeHostEnvironment();
        var manager = new SymlinkManager(env, service, Options.Create(new SymlinkOptions()), NullLogger<SymlinkManager>.Instance);

        var sources = new[] { "/src/A", "/src/B" };
        var results = await manager.CreateDirectoryLinksAsync(sources, "/dest");

        results.Should().HaveCount(2);
        results.Should().OnlyContain(r => r.Success);
        service.Created.Should().Contain((Path.Combine("/dest", "A"), "/src/A"));
        service.Created.Should().Contain((Path.Combine("/dest", "B"), "/src/B"));
    }

    [Fact]
    public async Task CreateDirectoryLinksAsync_marks_duplicate_names()
    {
        var service = new FakeSymlinkService();
        var env = new FakeHostEnvironment();
        var manager = new SymlinkManager(env, service, Options.Create(new SymlinkOptions()), NullLogger<SymlinkManager>.Instance);

        var sources = new[] { "/one/A", "/two/A" };
        var results = await manager.CreateDirectoryLinksAsync(sources, "/dest");

        results.Should().HaveCount(2);
        results[0].Success.Should().BeTrue();
        results[1].Success.Should().BeFalse();
        results[1].ErrorMessage.Should().Be("Duplicate folder name: A");
    }

    [Fact]
    public async Task CreateDirectoryLinksAsync_returns_error_for_relative_paths()
    {
        var service = new FakeSymlinkService();
        var env = new FakeHostEnvironment();
        var manager = new SymlinkManager(env, service, Options.Create(new SymlinkOptions()), NullLogger<SymlinkManager>.Instance);

        var results = await manager.CreateDirectoryLinksAsync(["relative"], "/dest");

        results[0].Success.Should().BeFalse();
        results[0].ErrorMessage.Should().Be("Paths must be absolute.");
    }

    [Fact]
    public async Task CreateFileLinkAsync_returns_error_when_not_development()
    {
        var service = new FakeSymlinkService();
        var env = new FakeHostEnvironment { EnvironmentName = Environments.Production };
        var manager = new SymlinkManager(env, service, Options.Create(new SymlinkOptions()), NullLogger<SymlinkManager>.Instance);

        var result = await manager.CreateFileLinkAsync("/src/a.txt", "/dest");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Developer mode not enabled.");
    }
}

internal sealed class FakeHostEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = Environments.Development;
    public string ApplicationName { get; set; } = "Test";
    public string ContentRootPath { get; set; } = "/";
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
