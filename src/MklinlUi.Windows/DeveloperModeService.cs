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
    private bool? _cached;

    public Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_cached.HasValue)
        {
            return Task.FromResult(_cached.Value);
        }

        if (!OperatingSystem.IsWindows())
        {
            _cached = false;
            return Task.FromResult(false);
        }

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AppModelUnlock");
            var value = key?.GetValue("AllowDevelopmentWithoutDevLicense");
            var enabled = value is int intVal && intVal != 0;
            _cached = enabled;
            return Task.FromResult(enabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to access developer mode registry.");
            throw new InvalidOperationException("Failed to access developer mode registry.", ex);
        }
    }
}
