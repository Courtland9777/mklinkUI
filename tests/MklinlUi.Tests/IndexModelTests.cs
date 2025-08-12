using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using MklinlUi.Core;
using MklinlUi.Fakes;
using MklinlUi.WebUI.Pages;
using Xunit;

namespace MklinlUi.Tests;

public class IndexModelTests
{
    [Fact]
    public async Task OnPostAsync_returns_error_for_invalid_filename()
    {
        var devService = new FakeDeveloperModeService();
        var manager = new SymlinkManager(devService, new FakeSymlinkService(), NullLogger<SymlinkManager>.Instance);
        var model = new IndexModel(manager, devService)
        {
            DestinationFolder = "/dest",
            SourceFilePaths = "/src/"
        };

        await model.OnPostAsync();

        model.Success.Should().BeFalse();
        model.Message.Should().Be("One or more file names are invalid.");
    }

    [Fact]
    public async Task OnPostAsync_returns_error_when_source_missing()
    {
        var devService = new FakeDeveloperModeService();
        var manager = new SymlinkManager(devService, new FakeSymlinkService(), NullLogger<SymlinkManager>.Instance);
        var model = new IndexModel(manager, devService)
        {
            DestinationFolder = "/dest",
            SourceFilePaths = "/src/missing.txt"
        };

        await model.OnPostAsync();

        model.Success.Should().BeFalse();
        model.Message.Should().Contain("Source file not found");
    }

    [Fact]
    public async Task OnPostAsync_creates_symlinks_when_sources_valid()
    {
        var devService = new FakeDeveloperModeService();
        var fakeService = new FakeSymlinkService();
        var manager = new SymlinkManager(devService, fakeService, NullLogger<SymlinkManager>.Instance);
        var tempFile = Path.GetTempFileName();

        var model = new IndexModel(manager, devService)
        {
            DestinationFolder = "/dest",
            SourceFilePaths = tempFile
        };

        await model.OnPostAsync();

        model.Success.Should().BeTrue();
        fakeService.Created.Should().ContainSingle();
        fakeService.Created[0].Should().Be((Path.Combine("/dest", Path.GetFileName(tempFile)), tempFile));

        File.Delete(tempFile);
    }
}