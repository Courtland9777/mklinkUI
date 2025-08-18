namespace MklinkUi.Core;

/// <summary>
/// Provides information about whether developer mode is enabled on the system.
/// </summary>
public interface IDeveloperModeService
{
    /// <summary>
    /// Gets a value indicating whether developer mode is enabled.
    /// </summary>
    bool IsEnabled { get; }
}
