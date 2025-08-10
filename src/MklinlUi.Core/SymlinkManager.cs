namespace MklinlUi.Core;

/// <summary>
///     Coordinates symbolic link creation using provided services.
/// </summary>
public sealed class SymlinkManager(IDeveloperModeService developerModeService, ISymlinkService symlinkService)
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
            return new SymlinkResult(false, "Developer mode not enabled.");

        try
        {
            return await symlinkService.CreateSymlinkAsync(linkPath, targetPath, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new SymlinkResult(false, ex.Message);
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

        if (!await developerModeService.IsEnabledAsync(cancellationToken).ConfigureAwait(false))
            return [new SymlinkResult(false, "Developer mode not enabled.")];

        try
        {
            return await symlinkService.CreateFileSymlinksAsync(sourceFiles, destinationFolder, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return [new SymlinkResult(false, ex.Message)];
        }
    }
}