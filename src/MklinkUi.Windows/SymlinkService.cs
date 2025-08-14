using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MklinkUi.Core;

namespace MklinkUi.Windows;

/// <summary>
///     Windows implementation of <see cref="ISymlinkService" />.
/// </summary>
public sealed class SymlinkService : ISymlinkService
{
    private readonly ILogger<SymlinkService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SymlinkService"/> class
    /// using a <see cref="NullLogger"/> instance.
    /// </summary>
    public SymlinkService()
        : this(null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SymlinkService"/> class.
    /// </summary>
    /// <param name="logger">Optional logger instance.</param>
    public SymlinkService(ILogger<SymlinkService>? logger)
    {
        _logger = logger ?? NullLogger<SymlinkService>.Instance;
    }

    public Task<SymlinkResult> CreateSymlinkAsync(string linkPath, string targetPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(linkPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);

        cancellationToken.ThrowIfCancellationRequested();

        if (File.Exists(linkPath) || Directory.Exists(linkPath))
            return Task.FromResult(new SymlinkResult(false, "Link already exists."));

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
            _logger.LogError(ex, "Failed to create symlink from {Link} to {Target}", linkPath, targetPath);
            return Task.FromResult(new SymlinkResult(false, GetMessage(ex)));
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
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(source))
            {
                results.Add(new SymlinkResult(false, "Invalid source."));
                continue;
            }

            var link = Path.Combine(destinationFolder, Path.GetFileName(source));

            if (File.Exists(link) || Directory.Exists(link))
            {
                results.Add(new SymlinkResult(false, "Link already exists."));
                continue;
            }

            try
            {
                File.CreateSymbolicLink(link, source);
                results.Add(new SymlinkResult(true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create file symlink from {Source} to {Link}", source, link);
                results.Add(new SymlinkResult(false, GetMessage(ex)));
            }
        }

        return Task.FromResult((IReadOnlyList<SymlinkResult>)results);
    }

    private static string GetMessage(Exception ex) => ex switch
    {
        UnauthorizedAccessException => "Access denied.",
        DirectoryNotFoundException or FileNotFoundException => "Path not found.",
        IOException => "I/O error occurred while creating the link.",
        _ => "Unexpected error occurred."
    };
}
