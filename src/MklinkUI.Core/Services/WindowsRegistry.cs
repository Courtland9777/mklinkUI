using Microsoft.Win32;

namespace MklinkUI.Core.Services;

public class WindowsRegistry : IRegistry
{
    public object? GetValue(string keyName, string valueName, object? defaultValue)
        => Registry.GetValue(keyName, valueName, defaultValue);
}
