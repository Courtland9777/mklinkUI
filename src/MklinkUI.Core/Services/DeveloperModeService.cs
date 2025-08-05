namespace MklinkUI.Core.Services;

public class DeveloperModeService : IDeveloperModeService
{
    private readonly IRegistry _registry;
    private const string KeyPath = @"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AppModelUnlock";
    private const string ValueName = "AllowDevelopmentWithoutDevLicense";
    private bool _isDeveloperModeEnabled;

    public DeveloperModeService(IRegistry registry)
    {
        _registry = registry;
        RefreshState();
    }

    public bool IsDeveloperModeEnabled() => _isDeveloperModeEnabled;

    public void RefreshState()
    {
        var value = _registry.GetValue(KeyPath, ValueName, 0);
        _isDeveloperModeEnabled = value is int intValue && intValue == 1;
    }
}
