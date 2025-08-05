using System.IO;
using FluentAssertions;
using MklinkUI.Core.Settings;
using Xunit;

namespace MklinkUI.Tests;

public class SettingsServiceTests
{
    [Fact]
    public void SaveAndLoad_PersistsSettings()
    {
        var temp = Path.GetTempFileName();
        try
        {
            var service = new SettingsService(temp);
            var settings = new AppSettings { Theme = ThemeOption.Dark, StartMinimizedToTray = true };
            service.Save(settings);

            var loaded = service.Load();
            loaded.Theme.Should().Be(ThemeOption.Dark);
            loaded.StartMinimizedToTray.Should().BeTrue();
        }
        finally
        {
            File.Delete(temp);
        }
    }
}
