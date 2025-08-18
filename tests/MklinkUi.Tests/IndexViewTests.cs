using System.IO;
using FluentAssertions;
using Xunit;

namespace MklinkUi.Tests;

public class IndexViewTests
{
    [Fact]
    public void IndexView_includes_antiforgery_attribute()
    {
        var dir = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(dir, "src", "MklinkUi.WebUI", "Pages", "Index.cshtml")))
        {
            var parent = Directory.GetParent(dir) ?? throw new InvalidOperationException("Could not locate repository root.");
            dir = parent.FullName;
        }

        var path = Path.Combine(dir, "src", "MklinkUi.WebUI", "Pages", "Index.cshtml");
        var content = File.ReadAllText(path);
        content.Should().Contain("asp-antiforgery=\"true\"");
    }
}

