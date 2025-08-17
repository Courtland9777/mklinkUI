using MklinkUi.Core;

namespace MklinkUi.Fakes;

/// <summary>
///     Records symlink requests without touching the file system.
/// </summary>
public sealed class FakeSymlinkService : ISymlinkService
{
    private readonly List<(string LinkPath, string TargetPath)> _created = [];

    /// <summary>
    ///     Gets all link/target pairs passed to the service.
    /// </summary>
    public IReadOnlyList<(string LinkPath, string TargetPath)> Created => _created;

    public Task<SymlinkResult> CreateFileLinkAsync(string sourceFile, string destinationFolder,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationFolder);
        if (!Path.IsPathFullyQualified(sourceFile) || !Path.IsPathFullyQualified(destinationFolder))
            throw new ArgumentException("Paths must be absolute.");

        var link = Path.Combine(destinationFolder, Path.GetFileName(sourceFile));
        if (_created.Any(c => string.Equals(c.LinkPath, link, StringComparison.OrdinalIgnoreCase)))
            return Task.FromResult(new SymlinkResult(false, "Link already exists."));

        _created.Add((link, sourceFile));
        return Task.FromResult(new SymlinkResult(true));
    }

    public Task<IReadOnlyList<SymlinkResult>> CreateDirectoryLinksAsync(IEnumerable<string> sourceFolders,
        string destinationFolder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceFolders);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationFolder);
        if (!Path.IsPathFullyQualified(destinationFolder))
            throw new ArgumentException("Paths must be absolute.");

        var results = new List<SymlinkResult>();
        foreach (var source in sourceFolders)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(source) || !Path.IsPathFullyQualified(source))
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
