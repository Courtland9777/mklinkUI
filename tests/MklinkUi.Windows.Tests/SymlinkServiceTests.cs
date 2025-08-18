using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MklinkUi.Windows;
using Xunit;

namespace MklinkUi.Windows.Tests;

public class SymlinkServiceTests
{
    [SkippableFact]
    public async Task CreateFileLinkAsync_ShouldCreateLink_WhenPathsAreValid()
    {
        Skip.IfNot(OperatingSystem.IsWindows());

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

    [SkippableFact]
    public async Task CreateFileLinkAsync_ShouldThrow_WhenCancelled()
    {
        Skip.IfNot(OperatingSystem.IsWindows());

        var service = new SymlinkService();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Func<Task> act = () => service.CreateFileLinkAsync("a", "b", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [SkippableFact]
    public async Task CreateDirectoryLinksAsync_ShouldCreateLink_WhenValid()
    {
        Skip.IfNot(OperatingSystem.IsWindows());

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

    [SkippableFact]
    public async Task CreateFileLinkAsync_ShouldReturnPathNotFound_WhenSourceMissing()
    {
        Skip.IfNot(OperatingSystem.IsWindows());

        var service = new SymlinkService();
        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(temp);
        var dest = Path.Combine(temp, "links");
        Directory.CreateDirectory(dest);

        var result = await service.CreateFileLinkAsync(Path.Combine(temp, "missing.txt"), dest);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Path not found.");
    }
}
