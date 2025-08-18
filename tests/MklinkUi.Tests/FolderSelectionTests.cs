using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MklinkUi.Core;
using MklinkUi.Fakes;
using MklinkUi.WebUI.Pages;
using Xunit;

namespace MklinkUi.Tests;

public class FolderSelectionTests
{
    [Fact]
    public async Task OnPostAsync_no_source_folders_is_noop()
    {
        var env = new FakeHostEnvironment();
        var service = new FakeSymlinkService();
        var manager = new SymlinkManager(env, service, Options.Create(new SymlinkOptions()), NullLogger<SymlinkManager>.Instance);
        var model = new IndexModel(manager, env, NullLogger<IndexModel>.Instance)
        {
            LinkType = "Folder",
            SourceFolders = string.Empty,
            DestinationFolder = string.Empty
        };

        var result = await model.OnPostAsync();

        result.Should().NotBeNull();
        model.Results.Should().BeEmpty();
    }
}
