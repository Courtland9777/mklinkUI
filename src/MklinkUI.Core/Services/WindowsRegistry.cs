using Microsoft.Win32;
using System.Runtime.Versioning;

namespace MklinkUI.Core.Services;

/// <summary>
/// Windows-specific implementation of <see cref="IRegistry"/>.
/// </summary>
[SupportedOSPlatform("windows")]
public class WindowsRegistry : IRegistry
{
    public object? GetValue(string keyName, string valueName, object? defaultValue)
        => Registry.GetValue(keyName, valueName, defaultValue);
}
