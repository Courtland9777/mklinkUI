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

    public Task<IReadOnlyList<SymlinkResult>> CreateFileSymlinksAsync(IEnumerable<string> sourceFiles,
        string destinationFolder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceFiles);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationFolder);

        cancellationToken.ThrowIfCancellationRequested();

        var results = new List<SymlinkResult>();
        foreach (var source in sourceFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(source))
            {
                results.Add(new SymlinkResult(false, "Invalid source."));
                continue;
            }

            var link = Path.Combine(destinationFolder, Path.GetFileName(source));
            if (_created.Any(c => string.Equals(c.LinkPath, link, StringComparison.OrdinalIgnoreCase)))
            {
                results.Add(new SymlinkResult(false, "Link already exists."));
                continue;
            }

            _created.Add((link, source));
            results.Add(new SymlinkResult(true));
        }

        return Task.FromResult((IReadOnlyList<SymlinkResult>)results);
    }
}