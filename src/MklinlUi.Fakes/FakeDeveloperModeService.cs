using MklinlUi.Core;

namespace MklinlUi.Fakes;

/// <summary>
/// Test double for <see cref="IDeveloperModeService"/>.
/// </summary>
public sealed class FakeDeveloperModeService : IDeveloperModeService
{
    /// <summary>
    /// Controls the value returned by <see cref="IsEnabledAsync"/>. Defaults to <c>true</c>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <inheritdoc />
    public Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Enabled);
}
