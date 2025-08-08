namespace MklinlUi.Core;

/// <summary>
///     Coordinates symbolic link creation using provided services.
/// </summary>
public class SymlinkManager(IDeveloperModeService developerModeService, ISymlinkService symlinkService)
{
    /// <summary>
    ///     Creates a symbolic link if developer mode is enabled.
    /// </summary>
    public async Task<SymlinkResult> CreateSymlinkAsync(string linkPath, string targetPath,
        CancellationToken cancellationToken = default)
    {
        if (!await developerModeService.IsEnabledAsync(cancellationToken).ConfigureAwait(false))
            return new SymlinkResult(false, "Developer mode not enabled.");

        return await symlinkService.CreateSymlinkAsync(linkPath, targetPath, cancellationToken).ConfigureAwait(false);
    }
}