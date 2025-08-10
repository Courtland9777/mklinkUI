#if WINDOWS
using FluentAssertions;
using MklinlUi.Windows;
using Xunit;
using System.IO;

namespace MklinlUi.Windows.Tests;

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

        var results = await service.CreateFileSymlinksAsync(new[] { source }, dest);

        results.Should().HaveCount(1);
        results[0].Success.Should().BeTrue();
        var link = Path.Combine(dest, "source.txt");
        File.Exists(link).Should().BeTrue();
        File.GetAttributes(link).HasFlag(FileAttributes.Directory).Should().BeFalse();
    }
}
#endif
