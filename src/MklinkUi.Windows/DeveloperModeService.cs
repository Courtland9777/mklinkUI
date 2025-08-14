using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Win32;
using MklinkUi.Core;

namespace MklinkUi.Windows;

/// <summary>
/// Windows implementation of <see cref="IDeveloperModeService"/>.
/// </summary>
public sealed class DeveloperModeService : IDeveloperModeService
{
    private readonly ILogger<DeveloperModeService> _logger;
    private bool? _cached;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeveloperModeService"/> class
    /// using a <see cref="NullLogger"/> instance.
    /// </summary>
    public DeveloperModeService()
        : this(null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeveloperModeService"/> class.
    /// </summary>
    /// <param name="logger">Optional logger instance.</param>
    public DeveloperModeService(ILogger<DeveloperModeService>? logger)
    {
        _logger = logger ?? NullLogger<DeveloperModeService>.Instance;
    }

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
