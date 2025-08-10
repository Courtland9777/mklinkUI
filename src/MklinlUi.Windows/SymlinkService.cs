using MklinlUi.Core;

namespace MklinlUi.Windows;

/// <summary>
///     Windows implementation of <see cref="ISymlinkService" />.
/// </summary>
public sealed class SymlinkService : ISymlinkService
{
    public Task<SymlinkResult> CreateSymlinkAsync(string linkPath, string targetPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(linkPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);

        try
        {
            if (Directory.Exists(targetPath))
                Directory.CreateSymbolicLink(linkPath, targetPath);
            else
                File.CreateSymbolicLink(linkPath, targetPath);

            return Task.FromResult(new SymlinkResult(true));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new SymlinkResult(false, ex.Message));
        }
    }
}