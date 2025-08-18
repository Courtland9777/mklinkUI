using System;
using System.Collections.Generic;
using System.Linq;
using MklinkUi.Core;

namespace MklinkUi.Fakes;

/// <summary>
///     Records symlink requests without touching the file system.
/// </summary>
public sealed class FakeSymlinkService : ISymlinkService
{
    private readonly IDeveloperModeService _developerMode;
    private readonly HashSet<string> _files = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _dirs = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _links = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<(string LinkPath, string TargetPath)> _created = [];

    public FakeSymlinkService(IDeveloperModeService? developerModeService = null)
        => _developerMode = developerModeService ?? new AlwaysOnDeveloperModeService();

    /// <summary>
    ///     Gets all link/target pairs passed to the service.
    /// </summary>
    public IReadOnlyList<(string LinkPath, string TargetPath)> Created => _created;

    public void SeedExistingFile(string path) => _files.Add(PathValidation.EnsureAbsolute(path, nameof(path)));
    public void SeedExistingDirectory(string path) => _dirs.Add(PathValidation.EnsureAbsolute(path, nameof(path)));
    public void SeedExistingLink(string path) => _links.Add(PathValidation.EnsureAbsolute(path, nameof(path)));

    public async Task<SymlinkResult> CreateFileLinkAsync(string sourceFile, string destinationFolder,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        sourceFile = PathValidation.EnsureAbsolute(sourceFile, nameof(sourceFile));
        destinationFolder = PathValidation.EnsureAbsolute(destinationFolder, nameof(destinationFolder));

        if (!_developerMode.IsEnabled)
            return new SymlinkResult(false, "Developer mode not enabled.", ErrorCodes.DevModeRequired);

        var linkPath = Path.Combine(destinationFolder, Path.GetFileName(sourceFile));
        linkPath = Path.GetFullPath(linkPath);

        if (_links.Contains(linkPath) || _files.Contains(linkPath))
            return new SymlinkResult(false, "Link already exists.", ErrorCodes.AlreadyExists);

        cancellationToken.ThrowIfCancellationRequested();

        _links.Add(linkPath);
        _created.Add((linkPath, sourceFile));
        return new SymlinkResult(true, LinkPath: linkPath);
    }

    public async Task<IReadOnlyList<SymlinkResult>> CreateDirectoryLinksAsync(IReadOnlyList<string> sourceFolders,
        string destinationFolder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceFolders);
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        destinationFolder = PathValidation.EnsureAbsolute(destinationFolder, nameof(destinationFolder));

        if (!_developerMode.IsEnabled)
        {
            var fails = Enumerable.Repeat(new SymlinkResult(false, "Developer mode not enabled.", ErrorCodes.DevModeRequired), sourceFolders.Count).ToArray();
            return fails;
        }

        var results = new List<SymlinkResult>(sourceFolders.Count);
        foreach (var source in sourceFolders)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string absSource;
            try
            {
                absSource = PathValidation.EnsureAbsolute(source, nameof(sourceFolders));
            }
            catch (ArgumentException)
            {
                results.Add(new SymlinkResult(false, "Paths must be absolute.", ErrorCodes.InvalidPath));
                continue;
            }

            var linkPath = Path.Combine(destinationFolder, Path.GetFileName(absSource));
            linkPath = Path.GetFullPath(linkPath);

            if (_links.Contains(linkPath) || _dirs.Contains(linkPath))
            {
                results.Add(new SymlinkResult(false, "Link already exists.", ErrorCodes.AlreadyExists));
                continue;
            }

            cancellationToken.ThrowIfCancellationRequested();

            _links.Add(linkPath);
            _created.Add((linkPath, absSource));
            results.Add(new SymlinkResult(true, LinkPath: linkPath));
        }

        return results;
    }

    private sealed class AlwaysOnDeveloperModeService : IDeveloperModeService
    {
        public bool IsEnabled => true;
    }
}
