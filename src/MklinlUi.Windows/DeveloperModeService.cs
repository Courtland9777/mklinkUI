using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Win32;
using MklinlUi.Core;

namespace MklinlUi.Windows;

/// <summary>
/// Windows implementation of <see cref="IDeveloperModeService"/>.
/// </summary>
public sealed class DeveloperModeService(ILogger<DeveloperModeService>? logger = null) : IDeveloperModeService
{
    private readonly ILogger<DeveloperModeService> _logger =
        logger ?? NullLogger<DeveloperModeService>.Instance;

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to access developer mode registry.");
            throw new InvalidOperationException("Failed to access developer mode registry.", ex);
        }
    }
}
