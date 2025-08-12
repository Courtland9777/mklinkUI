using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using MklinkUi.Core;
using MklinkUi.Fakes;
using MklinkUi.WebUI.Pages;
using System.IO;
using System.Threading;
using Xunit;

namespace MklinkUi.Tests;

public class IndexModelTests
{
    [Fact]
    public async Task OnPostAsync_returns_error_for_invalid_filename()
    {
        var devService = new FakeDeveloperModeService();
        var manager = new SymlinkManager(devService, new FakeSymlinkService(), NullLogger<SymlinkManager>.Instance);
        var invalidChar = Path.GetInvalidFileNameChars().First();
        var model = new IndexModel(manager, devService, NullLogger<IndexModel>.Instance)
        {
            DestinationFolder = "/dest",
            SourceFilePaths = $"/invalid{invalidChar}name.txt"
        };

        await model.OnPostAsync();

        model.Results.Should().HaveCount(1);
        var result = model.Results[0];
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("One or more file names are invalid.");
    }

    [Fact]
    public async Task OnPostAsync_returns_error_when_source_missing()
    {
        var devService = new FakeDeveloperModeService();
        var manager = new SymlinkManager(devService, new FakeSymlinkService(), NullLogger<SymlinkManager>.Instance);
        var model = new IndexModel(manager, devService, NullLogger<IndexModel>.Instance)
        {
            DestinationFolder = "/dest",
            SourceFilePaths = Path.Combine(Path.GetTempPath(), "missing.txt")
        };

        await model.OnPostAsync();

        model.Results.Should().HaveCount(1);
        var result = model.Results[0];
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Source file not found");
    }

    [Fact]
    public async Task OnPostAsync_returns_error_when_create_symlink_throws()
    {
        var devService = new ThrowingDeveloperModeService();
        var manager = new SymlinkManager(devService, new FakeSymlinkService(), NullLogger<SymlinkManager>.Instance);
        var model = new IndexModel(manager, devService, NullLogger<IndexModel>.Instance)
        {
            LinkType = "Folder",
            SourcePath = "/src",
            DestinationPath = "/dest"
        };

        await model.OnPostAsync();

        model.Results.Should().HaveCount(1);
        var result = model.Results[0];
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("An unexpected error occurred while creating the symlink.");
    }

    [Fact]
    public async Task OnPostAsync_returns_error_when_create_file_symlinks_throws()
    {
        var tempFile = Path.GetTempFileName();
        var devService = new ThrowingDeveloperModeService();
        var manager = new SymlinkManager(devService, new FakeSymlinkService(), NullLogger<SymlinkManager>.Instance);
        var model = new IndexModel(manager, devService, NullLogger<IndexModel>.Instance)
        {
            DestinationFolder = "/dest",
            SourceFilePaths = tempFile
        };

        await model.OnPostAsync();

        model.Results.Should().HaveCount(1);
        var result = model.Results[0];
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("An unexpected error occurred while creating file symlinks.");
    }


    private sealed class ThrowingDeveloperModeService : IDeveloperModeService
    {
        private bool first = true;

        public Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default)
        {
            if (first)
            {
                first = false;
                return Task.FromResult(true);
            }

            throw new InvalidOperationException("Failure");
        }
    }

}
