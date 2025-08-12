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

        try
        {
            return await symlinkService
                .CreateFileSymlinksAsync(sources, destinationFolder, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create file symlinks in {DestinationFolder}", destinationFolder);
            return [.. sources.Select(_ => new SymlinkResult(false, "Failed to create symlinks."))];
        }
    }
}