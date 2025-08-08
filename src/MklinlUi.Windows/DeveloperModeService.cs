using Microsoft.Win32;
using MklinlUi.Core;

namespace MklinlUi.Windows;

/// <summary>
/// Windows implementation of <see cref="IDeveloperModeService"/>.
/// </summary>
public sealed class DeveloperModeService : IDeveloperModeService
{
    public Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!OperatingSystem.IsWindows())
        {
            return Task.FromResult(false);
        }

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AppModelUnlock");
            var value = key?.GetValue("AllowDevelopmentWithoutDevLicense");
            return Task.FromResult(value is int intVal && intVal != 0);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
