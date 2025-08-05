namespace MklinkUI.Core.Services;

public class DeveloperModeService : IDeveloperModeService
{
    private readonly IRegistry _registry;
    private const string KeyPath = @"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AppModelUnlock";
    private const string ValueName = "AllowDevelopmentWithoutDevLicense";

    public DeveloperModeService(IRegistry registry)
    {
        _registry = registry;
    }

    public bool IsDeveloperModeEnabled()
    {
        var value = _registry.GetValue(KeyPath, ValueName, 0);
        return value is int intValue && intValue == 1;
    }
}
