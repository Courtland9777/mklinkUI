using System.IO;
using FluentAssertions;
using Xunit;

namespace MklinlUi.Tests;

public class IndexViewTests
{
    [Fact]
    public void IndexView_includes_antiforgery_attribute()
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "src", "MklinlUi.WebUI", "Pages", "Index.cshtml"));
        File.Exists(path).Should().BeTrue();
        var content = File.ReadAllText(path);
        content.Should().Contain("asp-antiforgery=\"true\"");
    }
}

