#if WINDOWS
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace MklinkUi.Windows.Tests;

public class SymlinkServiceTests
{
    [Fact]
    public async Task CreateFileSymlinksAsync_creates_file_links()
    {
        var service = new SymlinkService();
        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(temp);
        var source = Path.Combine(temp, "source.txt");
        await File.WriteAllTextAsync(source, "data");
        var dest = Path.Combine(temp, "links");
        Directory.CreateDirectory(dest);

        var results = await service.CreateFileSymlinksAsync([source], dest);

        results.Should().HaveCount(1);
        results[0].Success.Should().BeTrue();
        var link = Path.Combine(dest, "source.txt");
        File.Exists(link).Should().BeTrue();
        File.GetAttributes(link).HasFlag(FileAttributes.Directory).Should().BeFalse();
    }

    [Fact]
    public async Task CreateSymlinkAsync_throws_when_cancelled()
    {
        var service = new SymlinkService();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var act = () => service.CreateSymlinkAsync("link", "target", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task CreateSymlinkAsync_returns_failure_if_link_exists()
    {
        var service = new SymlinkService();
        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(temp);
        var target = Path.Combine(temp, "target.txt");
        await File.WriteAllTextAsync(target, "data");
        var link = Path.Combine(temp, "link.txt");
        await File.WriteAllTextAsync(link, "existing");

        var result = await service.CreateSymlinkAsync(link, target);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Link already exists.");
        File.GetAttributes(link).HasFlag(FileAttributes.ReparsePoint).Should().BeFalse();
    }
}
#endif