#if WINDOWS
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using MklinkUi.Windows;

namespace MklinkUi.Windows.Tests;

public class SymlinkServiceTests
{
    [Fact]
    public async Task CreateFileLinkAsync_creates_file_link()
    {
        var service = new SymlinkService();
        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(temp);
        var source = Path.Combine(temp, "source.txt");
        await File.WriteAllTextAsync(source, "data");
        var dest = Path.Combine(temp, "links");
        Directory.CreateDirectory(dest);

        var result = await service.CreateFileLinkAsync(source, dest);

        result.Success.Should().BeTrue();
        var link = Path.Combine(dest, "source.txt");
        File.Exists(link).Should().BeTrue();
        File.GetAttributes(link).HasFlag(FileAttributes.Directory).Should().BeFalse();
    }

    [Fact]
    public async Task CreateFileLinkAsync_throws_when_cancelled()
    {
        var service = new SymlinkService();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var act = () => service.CreateFileLinkAsync("a", "b", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task CreateDirectoryLinksAsync_creates_directory_link()
    {
        var service = new SymlinkService();
        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(temp);
        var source = Path.Combine(temp, "sourceDir");
        Directory.CreateDirectory(source);
        var dest = Path.Combine(temp, "links");
        Directory.CreateDirectory(dest);

        var results = await service.CreateDirectoryLinksAsync([source], dest);

        results.Should().HaveCount(1);
        results[0].Success.Should().BeTrue();
        var link = Path.Combine(dest, "sourceDir");
        Directory.Exists(link).Should().BeTrue();
    }
}
#endif