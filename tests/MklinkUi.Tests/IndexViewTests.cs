using System.IO;
using FluentAssertions;
using Xunit;

namespace MklinkUi.Tests;

public class IndexViewTests
{
    [Fact]
    public void IndexView_includes_antiforgery_attribute()
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "src", "MklinkUi.WebUI", "Pages", "Index.cshtml"));
        File.Exists(path).Should().BeTrue();
        var content = File.ReadAllText(path);
        content.Should().Contain("asp-antiforgery=\"true\"");
    }
}

