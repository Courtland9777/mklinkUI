using FluentAssertions;
using MklinlUi.Core;
using MklinlUi.Fakes;
using Xunit;

namespace MklinlUi.Tests;

public class FileBatchSymlinkTests
{
    [Fact]
    public async Task CreateFileSymlinksAsync_creates_links_for_each_file()
    {
        var service = new FakeSymlinkService();
        var manager = new SymlinkManager(new FakeDeveloperModeService(), service);

        const string dest = "/dest";
        var sources = new[] { Path.Combine("/src", "a.txt"), Path.Combine("/src", "b.txt") };
        var results = await manager.CreateFileSymlinksAsync(sources, dest);

        results.Should().HaveCount(2);
        results.Should().OnlyContain(r => r.Success);
        service.Created.Should().Contain((Path.Combine(dest, "a.txt"), sources[0]));
        service.Created.Should().Contain((Path.Combine(dest, "b.txt"), sources[1]));
    }

    [Fact]
    public async Task CreateFileSymlinksAsync_skips_on_name_collision()
    {
        var service = new FakeSymlinkService();
        var manager = new SymlinkManager(new FakeDeveloperModeService(), service);

        var sources = new[] { "/src/a.txt", "/other/a.txt" }; // same file name
        var results = await manager.CreateFileSymlinksAsync(sources, "/dest");

        results.Should().HaveCount(2);
        results[0].Success.Should().BeTrue();
        results[1].Success.Should().BeFalse();
    }

    [Fact]
    public async Task CreateFileSymlinksAsync_returns_failure_for_invalid_source()
    {
        var service = new FakeSymlinkService();
        var manager = new SymlinkManager(new FakeDeveloperModeService(), service);

        var results = await manager.CreateFileSymlinksAsync([string.Empty], "/dest");

        results.Should().HaveCount(1);
        results[0].Success.Should().BeFalse();
    }
}