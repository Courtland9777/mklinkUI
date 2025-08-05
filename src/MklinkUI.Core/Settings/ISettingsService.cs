namespace MklinkUI.Core.Settings;

public interface ISettingsService
{
    AppSettings Load();
    void Save(AppSettings settings);
}
