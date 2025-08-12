namespace MklinkUi.Core;

/// <summary>
/// Provides information about whether developer mode is enabled on the current machine.
/// </summary>
public interface IDeveloperModeService
{
    /// <summary>
    /// Determines whether developer mode is enabled.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if developer mode is enabled; otherwise, <c>false</c>.</returns>
    Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default);
}
