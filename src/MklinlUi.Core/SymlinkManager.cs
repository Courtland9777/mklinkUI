using Microsoft.Extensions.Logging;

namespace MklinlUi.Core;

/// <summary>
///     Coordinates symbolic link creation using provided services.
/// </summary>
public sealed class SymlinkManager(
    IDeveloperModeService developerModeService,
    ISymlinkService symlinkService,
    ILogger<SymlinkManager> logger)
{
    /// <summary>
    ///     Creates a symbolic link if developer mode is enabled.
    /// </summary>
    public async Task<SymlinkResult> CreateSymlinkAsync(string linkPath, string targetPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(linkPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);

        if (!await developerModeService.IsEnabledAsync(cancellationToken).ConfigureAwait(false))
        {
            logger.LogWarning("Developer mode not enabled.");
            return new SymlinkResult(false, "Developer mode not enabled.");
        }

        try
        {
            return await symlinkService.CreateSymlinkAsync(linkPath, targetPath, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create symlink from {LinkPath} to {TargetPath}", linkPath, targetPath);
            return new SymlinkResult(false, "Failed to create symlink.");
        }
    }

    /// <summary>
    ///     Creates multiple file symbolic links within a destination folder if developer mode is enabled.
    /// </summary>
    public async Task<IReadOnlyList<SymlinkResult>> CreateFileSymlinksAsync(IEnumerable<string> sourceFiles,
        string destinationFolder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceFiles);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationFolder);

        var sources = sourceFiles.ToList();
        if (!await developerModeService.IsEnabledAsync(cancellationToken).ConfigureAwait(false))
        {
            logger.LogWarning("Developer mode not enabled.");
            return [.. sources.Select(_ => new SymlinkResult(false, "Developer mode not enabled."))];
        }

        var groups = sources
            .Select((s, i) => (Source: s, Index: i, Name: Path.GetFileName(s)))
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase);

        var unique = new List<(string Source, int Index)>();
        var results = new SymlinkResult[sources.Count];

        foreach (var group in groups)
        {
            var first = group.First();
            unique.Add((first.Source, first.Index));
            foreach (var item in group.Skip(1))
                results[item.Index] = new SymlinkResult(false, $"Duplicate file name: {group.Key}");
        }

        try
        {
            var serviceResults = await symlinkService
                .CreateFileSymlinksAsync(unique.Select(u => u.Source), destinationFolder, cancellationToken)
                .ConfigureAwait(false);

            if (serviceResults.Count != unique.Count)
            {
                logger.LogError("Symlink service returned {ResultCount} results for {SourceCount} sources.",
                    serviceResults.Count, unique.Count);
                var error = "Failed to create symlinks.";
                foreach (var (_, index) in unique)
                    results[index] = new SymlinkResult(false, error);
                return results;
            }

            for (var i = 0; i < unique.Count; i++)
                results[unique[i].Index] = serviceResults[i];

            return results;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create file symlinks in {DestinationFolder}", destinationFolder);
            var error = ex.Message;
            foreach (var (_, index) in unique)
                results[index] = new SymlinkResult(false, error);
            return results;
        }
    }
}