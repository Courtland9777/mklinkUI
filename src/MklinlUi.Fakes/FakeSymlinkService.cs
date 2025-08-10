using MklinlUi.Core;

namespace MklinlUi.Fakes;

/// <summary>
///     Records symlink requests without touching the file system.
/// </summary>
public sealed class FakeSymlinkService : ISymlinkService
{
    private readonly List<(string LinkPath, string TargetPath)> _created = [];

    /// <summary>
    ///     Gets all link/target pairs passed to <see cref="CreateSymlinkAsync" />.
    /// </summary>
    public IReadOnlyList<(string LinkPath, string TargetPath)> Created => _created;

    public Task<SymlinkResult> CreateSymlinkAsync(string linkPath, string targetPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(linkPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);
        _created.Add((linkPath, targetPath));
        return Task.FromResult(new SymlinkResult(true));
    }
}