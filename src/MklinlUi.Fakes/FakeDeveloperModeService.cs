using MklinlUi.Core;

namespace MklinlUi.Fakes;

/// <summary>
/// Fake implementation of <see cref="IDeveloperModeService"/> for tests and non-Windows environments.
/// </summary>
public class FakeDeveloperModeService : IDeveloperModeService
{
    /// <summary>
    /// Gets or sets a value indicating whether developer mode is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    public Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Enabled);
}
