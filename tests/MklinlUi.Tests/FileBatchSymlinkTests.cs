using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using MklinlUi.Core;
using MklinlUi.Fakes;
using System.Threading;
using Xunit;

namespace MklinlUi.Tests;

public class FileBatchSymlinkTests
{
    [Fact]
    public async Task CreateFileSymlinksAsync_creates_links_for_each_file()
    {
        var service = new FakeSymlinkService();
        var manager = new SymlinkManager(new FakeDeveloperModeService(), service, NullLogger<SymlinkManager>.Instance);

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
        var manager = new SymlinkManager(new FakeDeveloperModeService(), service, NullLogger<SymlinkManager>.Instance);

        var sources = new[] { "/src/a.txt", "/other/a.txt" }; // same file name
        var results = await manager.CreateFileSymlinksAsync(sources, "/dest");

        results.Should().HaveCount(2);
        results[0].Success.Should().BeTrue();
        results[1].Success.Should().BeFalse();
        results[1].ErrorMessage.Should().Be("Duplicate file name: a.txt");
        service.Created.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateFileSymlinksAsync_returns_failure_for_invalid_source()
    {
        var service = new FakeSymlinkService();
        var manager = new SymlinkManager(new FakeDeveloperModeService(), service, NullLogger<SymlinkManager>.Instance);

        var results = await manager.CreateFileSymlinksAsync([string.Empty], "/dest");

        results.Should().HaveCount(1);
        results[0].Success.Should().BeFalse();
    }

    [Fact]
    public async Task CreateFileSymlinksAsync_propagates_service_exception()
    {
        var service = new FailingSymlinkService();
        var manager = new SymlinkManager(new FakeDeveloperModeService(), service, NullLogger<SymlinkManager>.Instance);

        var results = await manager.CreateFileSymlinksAsync(["/src/a.txt"], "/dest");

        results.Should().HaveCount(1);
        results[0].Success.Should().BeFalse();
        results[0].ErrorMessage.Should().Be("boom");
    }

    private sealed class FailingSymlinkService : ISymlinkService
    {
        public Task<SymlinkResult> CreateSymlinkAsync(string linkPath, string targetPath, CancellationToken cancellationToken = default)
            => Task.FromResult(new SymlinkResult(true));

        public Task<IReadOnlyList<SymlinkResult>> CreateFileSymlinksAsync(IEnumerable<string> sourceFiles, string destinationFolder, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("boom");
    }
  [Fact]
    public async Task CreateFileSymlinksAsync_throws_when_cancelled_before_start()
    {
        var service = new FakeSymlinkService();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            service.CreateFileSymlinksAsync(["/src/a.txt"], "/dest", cts.Token));
    }

    [Fact]
    public async Task CreateFileSymlinksAsync_throws_when_cancelled_during_iteration()
    {
        var service = new FakeSymlinkService();
        using var cts = new CancellationTokenSource();
        IEnumerable<string> Sources()
        {
            yield return "/src/a.txt";
            cts.Cancel();
            yield return "/src/b.txt";
        }

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            service.CreateFileSymlinksAsync(Sources(), "/dest", cts.Token));
    }
}