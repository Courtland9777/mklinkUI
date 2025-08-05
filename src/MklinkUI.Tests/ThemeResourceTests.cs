using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace MklinkUI.Tests;

public class ThemeResourceTests
{
    [Theory]
    [InlineData("Light.xaml")]
    [InlineData("Dark.xaml")]
    public void Theme_contains_control_styles(string themeFile)
    {
        var basePath = AppContext.BaseDirectory;
        var themePath = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", "..", "MklinkUI.App", "Themes", themeFile));
        var doc = XDocument.Load(themePath);
        var styles = doc.Root?.Elements().Where(e => e.Name.LocalName == "Style").ToList();

        Assert.NotNull(styles);
        Assert.Contains(styles, e => e.Attribute("TargetType")?.Value.Contains("Button") == true);
        Assert.Contains(styles, e => e.Attribute("TargetType")?.Value.Contains("TextBox") == true);
        Assert.Contains(styles, e => e.Attribute("TargetType")?.Value.Contains("ComboBox") == true);
    }
}

