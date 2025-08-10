using MklinlUi.Core;
using Serilog;

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

    public Task<IReadOnlyList<SymlinkResult>> CreateFileSymlinksAsync(IEnumerable<string> sourceFiles,
        string destinationFolder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceFiles);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationFolder);

        var results = new List<SymlinkResult>();

        foreach (var source in sourceFiles)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                results.Add(new SymlinkResult(false, "Invalid source."));
                continue;
            }

            var link = Path.Combine(destinationFolder, Path.GetFileName(source));

            if (File.Exists(link) || Directory.Exists(link))
            {
                Log.Warning("Link already exists for {Link}", link);
                results.Add(new SymlinkResult(false, "Link already exists."));
                continue;
            }

            try
            {
                File.CreateSymbolicLink(link, source);
                Log.Information("Created link {Link} -> {Target}", link, source);
                results.Add(new SymlinkResult(true));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create link {Link}", link);
                results.Add(new SymlinkResult(false, ex.Message));
            }
        }

        return Task.FromResult((IReadOnlyList<SymlinkResult>)results);
    }
}