using MklinkUI.Core.Services;

namespace MklinkUI.Core.Settings;

public class ThemeService : IThemeService
{
    private readonly IRegistry _registry;
    private const string Key = @"HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize";
    private const string Value = "AppsUseLightTheme";

    public ThemeService(IRegistry registry)
    {
        _registry = registry;
    }

    public ThemeOption GetSystemTheme()
    {
        var val = _registry.GetValue(Key, Value, 1);
        return val is int i && i == 0 ? ThemeOption.Dark : ThemeOption.Light;
    }

    public ThemeOption ResolveTheme(ThemeOption preference)
        => preference == ThemeOption.System ? GetSystemTheme() : preference;
}
