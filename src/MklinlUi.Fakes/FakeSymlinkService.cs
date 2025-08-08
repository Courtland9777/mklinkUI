using MklinlUi.Core;

namespace MklinlUi.Fakes;

/// <summary>
/// Records symlink requests without touching the file system.
/// </summary>
public sealed class FakeSymlinkService : ISymlinkService
{
    /// <summary>
    /// Gets all link/target pairs passed to <see cref="CreateSymlinkAsync"/>.
    /// </summary>
    public List<(string linkPath, string targetPath)> Created { get; } = [];

    public Task<SymlinkResult> CreateSymlinkAsync(string linkPath, string targetPath, CancellationToken cancellationToken = default)
    {
        Created.Add((linkPath, targetPath));
        return Task.FromResult(new SymlinkResult(true));
    }
}

