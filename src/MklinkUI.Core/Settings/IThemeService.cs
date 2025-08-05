namespace MklinkUI.Core.Settings;

public interface IThemeService
{
    ThemeOption GetSystemTheme();
    ThemeOption ResolveTheme(ThemeOption preference);
}
