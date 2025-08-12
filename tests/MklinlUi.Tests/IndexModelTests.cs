using FluentAssertions;
using Microsoft.AspNetCore.Http;
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
            SourceFiles = [CreateFormFile("")]
        };

        var result = await model.OnPostAsync();

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
            SourceFiles = [CreateFormFile("missing.txt")]
        };

        var result = await model.OnPostAsync();

        model.Success.Should().BeFalse();
        model.Message.Should().Contain("Source file not found");
    }

    private static FormFile CreateFormFile(string fileName)
    {
        var stream = new MemoryStream();
        return new FormFile(stream, 0, 0, fileName, fileName);
    }
}